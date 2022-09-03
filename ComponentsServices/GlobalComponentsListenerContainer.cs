using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public sealed class GlobalComponentsListenerContainer<T> : IRemoveSystemListener, IDisposable where T : IComponent
    {
        private struct GlobalListener
        {
            public readonly IReactComponentGlobal<T> Action;
            public readonly ISystem Listener;

            public GlobalListener(ISystem listener, IReactComponentGlobal<T> action)
            {
                Listener = listener;
                Action = action;
            }

            public override bool Equals(object obj)
            {
                return obj is GlobalListener container &&
                       Listener.SystemGuid.Equals(container.Listener.SystemGuid);
            }

            public override int GetHashCode()
            {
                return Listener.SystemGuid.GetHashCode();
            }
        }

        private Dictionary<Guid, GlobalListener> listeners = new Dictionary<Guid, GlobalListener>(16);
        private Queue<(T component, bool Add)> invokeComponents = new Queue<(T component, bool Add)>(16);
        private Queue<Guid> listenersToRemove = new Queue<Guid>(4);

        private bool isDirty;
        private bool isAdded;
        private bool inProgress;
        private World world;

        public GlobalComponentsListenerContainer(ISystem listener, IReactComponentGlobal<T> action)
        {
            listeners.Add(listener.SystemGuid, new GlobalListener(listener, action));
            world = listener.Owner.World;
            world.GlobalUpdateSystem.FinishUpdate += ProcessInvoke;
        }

        public void ListenCommand(ISystem listener, IReactComponentGlobal<T> action)
        {
            if (listeners.ContainsKey(listener.SystemGuid))
                return;

            listeners.Add(listener.SystemGuid, new GlobalListener(listener, action));
        }

        public void Invoke(T component, bool isAdded)
        {
            ProcessRemove();

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

                actualListener.Action.ComponentReactGlobal(data.component, data.Add);
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