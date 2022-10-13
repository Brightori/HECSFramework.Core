using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public sealed class LocalComponentsListenerContainer<T> : IDisposable, IRemoveSystemListener where T : IComponent
    {
        private struct LocalListener
        {
            public readonly IReactComponentLocal<T> Action;
            public readonly ISystem Listener;

            public LocalListener(ISystem listener, IReactComponentLocal<T> action)
            {
                Listener = listener;
                Action = action;
            }

            public override bool Equals(object obj)
            {
                return obj is LocalListener container &&
                       Listener.SystemGuid.Equals(container.Listener.SystemGuid);
            }

            public override int GetHashCode()
            {
                return Listener.SystemGuid.GetHashCode();
            }
        }

        private Dictionary<Guid, LocalListener> listeners = new Dictionary<Guid, LocalListener>(16);

        private Queue<Guid> listenersToRemove = new Queue<Guid>(4);
        private Queue<(T component, bool Add)> invokeComponents = new Queue<(T component, bool Add)>(16);
        private bool isDirty;
        private bool isAdded;
        private bool inProgress;
        private World world;

        public LocalComponentsListenerContainer(ISystem listener, IReactComponentLocal<T> action)
        {
            listeners.Add(listener.SystemGuid, new LocalListener(listener, action));
            world = listener.Owner.World;
            world.GlobalUpdateSystem.FinishUpdate += ProcessInvoke;
        }

        public void ListenCommand(ISystem listener, IReactComponentLocal<T> action)
        {
            if (listeners.ContainsKey(listener.SystemGuid))
                return;

            listeners.Add(listener.SystemGuid, new LocalListener(listener, action));
        }

        public void Invoke(T component, bool isAdded)
        {
            if (inProgress)
            {
                invokeComponents.Enqueue((component, isAdded));
                this.isAdded = true;
            }
            else
                InvokeToListeners((component, isAdded));
        }

        public void ProcessInvoke()
        {
            if (!isAdded) return;
            ProcessRemove();

            while (invokeComponents.Count > 0)
            {
                var data = invokeComponents.Dequeue();
                InvokeToListeners(data);
            }

            ProcessRemove();
            isAdded = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokeToListeners((T component, bool Add) data)
        {
            inProgress = true;

            foreach (var listener in listeners)
            {
                var actualListener = listener.Value;

                if (actualListener.Listener == null || !actualListener.Listener.Owner.IsAlive)
                {
                    listenersToRemove.Enqueue(listener.Value.Listener.SystemGuid);
                    isDirty = true;
                    continue;
                }

                if (actualListener.Listener.Owner.IsPaused)
                    continue;

                actualListener.Action.ComponentReact(data.component, data.Add);
            }
            inProgress = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessRemove()
        {
            if (isDirty)
            {
                while (listenersToRemove.Count > 0)
                {
                    var remove = listenersToRemove.Dequeue();
                    listeners.Remove(remove);
                }

                isDirty = false;
            }
        }

        public void RemoveListener(ISystem listener)
        {
            if (listeners.ContainsKey(listener.SystemGuid))
            {
                listenersToRemove.Enqueue(listener.SystemGuid);
                isDirty = true;
            }
        }

        public void Dispose()
        {
            listeners.Clear();
            invokeComponents.Clear();
            listenersToRemove.Clear();
            world.GlobalUpdateSystem.FinishUpdate -= ProcessInvoke;
            world = null;
        }
    }

    public sealed class LocalComponentsListenerContainer : IDisposable, IRemoveSystemListener
    {
        private struct LocalListener
        {
            public readonly IReactComponentLocal Action;
            public readonly ISystem Listener;

            public LocalListener(ISystem listener, IReactComponentLocal action)
            {
                Listener = listener;
                Action = action;
            }

            public override bool Equals(object obj)
            {
                return obj is LocalListener container &&
                       Listener.SystemGuid.Equals(container.Listener.SystemGuid);
            }

            public override int GetHashCode()
            {
                return Listener.SystemGuid.GetHashCode();
            }
        }

        private Dictionary<Guid, LocalListener> listeners = new Dictionary<Guid, LocalListener>(16);

        private Queue<Guid> listenersToRemove = new Queue<Guid>(4);
        private Queue<(IComponent component, bool Add)> invokeComponents = new Queue<(IComponent component, bool Add)>(16);
        private bool isDirty;
        private bool isAdded;
        private World world;
        private bool isInited;
        private bool inProgress;

        public void ListenCommand(ISystem listener, IReactComponentLocal action)
        {
            if (!isInited)
                Init(listener);

            if (listeners.ContainsKey(listener.SystemGuid))
                return;

            listeners.Add(listener.SystemGuid, new LocalListener(listener, action));
        }

        private void Init(ISystem listener)
        {
            world = listener.Owner.World;
            world.GlobalUpdateSystem.FinishUpdate += ProcessInvoke;
            isInited = true;
        }

        public void Invoke(IComponent component, bool isAdded)
        {
            if (inProgress)
            {
                invokeComponents.Enqueue((component, isAdded));
                this.isAdded = true;
            }
            else
                InvokeToListeners((component, isAdded));
        }

        public void ProcessInvoke()
        {
            if (!isAdded) return;

            ProcessRemove();

            while (invokeComponents.Count > 0)
            {
                var data = invokeComponents.Dequeue();
                InvokeToListeners(data);
            }

            ProcessRemove();
            isAdded = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokeToListeners((IComponent component, bool Add) data)
        {
            inProgress = true;

            foreach (var listener in listeners)
            {
                var actualListener = listener.Value;

                if (actualListener.Listener == null || !actualListener.Listener.Owner.IsAlive)
                {
                    listenersToRemove.Enqueue(listener.Value.Listener.SystemGuid);
                    isDirty = true;
                    continue;
                }

                if (actualListener.Listener.Owner.IsPaused)
                    continue;

                actualListener.Action.ComponentReactLocal(data.component, data.Add);
            }
            inProgress = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessRemove()
        {
            if (isDirty)
            {
                while (listenersToRemove.Count > 0)
                {
                    var remove = listenersToRemove.Dequeue();
                    listeners.Remove(remove);
                }

                isDirty = false;
            }
        }

        public void RemoveListener(ISystem listener)
        {
            if (listeners.ContainsKey(listener.SystemGuid))
            {
                listenersToRemove.Enqueue(listener.SystemGuid);
                isDirty = true;
            }
        }

        public void Dispose()
        {
            listeners.Clear();
            invokeComponents.Clear();
            listenersToRemove.Clear();
            world.GlobalUpdateSystem.FinishUpdate -= ProcessInvoke;
            world = null;
        }
    }
}