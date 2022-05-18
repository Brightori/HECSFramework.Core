﻿using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public sealed class LocalComponentsListenerContainer<T> : IDisposable, IRemoveSystemListener
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
            this.isAdded = true;
            invokeComponents.Enqueue((component, isAdded));
        }

        public void ProcessInvoke()
        {
            if (!isAdded) return;

            ProcessRemove();

            while (invokeComponents.Count > 0)
            {
                var data = invokeComponents.Dequeue();

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
            }

            ProcessRemove();
            isAdded = false;
        }

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