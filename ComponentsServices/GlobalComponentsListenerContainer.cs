using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public sealed class GlobalComponentsListenerContainer<T> : IRemoveSystemListener where T : IComponent
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

        private Queue<Guid> listenersToRemove = new Queue<Guid>(4);

        public GlobalComponentsListenerContainer(ISystem listener, IReactComponentGlobal<T> action)
        {
            listeners.Add(listener.SystemGuid, new GlobalListener(listener, action));
        }

        public void ListenCommand(ISystem listener, IReactComponentGlobal<T> action)
        {
            if (listeners.ContainsKey(listener.SystemGuid))
                return;

            listeners.Add(listener.SystemGuid, new GlobalListener(listener, action));
        }

        public void Invoke(T component, bool isAdded)
        {
            foreach (var listener in listeners)
            {
                var actualListener = listener.Value;

                if (actualListener.Listener == null || !actualListener.Listener.Owner.IsAlive)
                {
                    listenersToRemove.Enqueue(listener.Value.Listener.SystemGuid);
                    continue;
                }

                if (actualListener.Listener.Owner.IsPaused)
                    continue;
                
                actualListener.Action.ComponentReactGlobal(component, isAdded);
            }

            while (listenersToRemove.Count > 0)
            {
                var remove = listenersToRemove.Dequeue();
                listeners.Remove(remove);
            }
        }

        public void RemoveListener(ISystem listener)
        {
            if (listeners.ContainsKey(listener.SystemGuid))
                listeners.Remove(listener.SystemGuid);
        }
    }
}