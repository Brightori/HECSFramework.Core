using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core
{
    public class EntityCommandService : ICommandService
    {
        private Dictionary<Type, object> commandListeners = new Dictionary<Type, object>();

        public void Invoke<T>(T data) 
        {
            var key = typeof(T);
            if (!commandListeners.TryGetValue(key, out var commandListenerContainer))
                return;
            var eventContainer = (CommandListener<T>)commandListenerContainer;
            eventContainer.Invoke(data);
        }

        public void ReleaseListener(ISystem listener)
        {
            foreach (var l in commandListeners)
                (l.Value as IRemoveSystemListener).RemoveListener(listener);
        }

        public void RemoveListener<T>(ISystem listener) 
        {
            var key = typeof(T);
            if (commandListeners.TryGetValue(key, out var container))
            {
                var eventContainer = (CommandListener<T>)container;
                eventContainer.RemoveListener(listener);
            }
        }

        public void AddListener<T>(ISystem listener, Action<T> action) 
        {
            var key = typeof(T);

            if (commandListeners.ContainsKey(key))
            {
                var lr = (CommandListener<T>)commandListeners[key];
                lr.ListenCommand(listener, action);
                return;
            }
            commandListeners.Add(key, new CommandListener<T>(listener, action));
        }

        public void Dispose()
        {
            commandListeners.Clear();
        }
    }
}

namespace HECSFramework.Core
{
    public sealed class CommandListener<T> : IRemoveSystemListener 
    {
        private struct ListenerActionContainer
        {
            public readonly Action<T> Action;
            public readonly ISystem Listener;
            public Guid Guid;

            public ListenerActionContainer(ISystem listener, Action<T> action)
            {
                Listener = listener;
                Action = action;
                Guid = listener.SystemGuid;
            }

            public override bool Equals(object obj)
            {
                return obj is ListenerActionContainer container &&
                       Guid.Equals(container.Guid);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Guid);
            }
        }

        private Dictionary<Guid, ListenerActionContainer> listeners = new Dictionary<Guid, ListenerActionContainer>();

        private Queue<Guid> listenersToRemove = new Queue<Guid>(4);

        public CommandListener(ISystem listener, Action<T> action)
        {
            listeners.Add(listener.SystemGuid, new ListenerActionContainer(listener, action));
        }

        public void ListenCommand(ISystem listener, Action<T> action)
        {
            var listenerGuid = listener.SystemGuid;

            if (listeners.ContainsKey(listenerGuid))
                return;

            listeners.Add(listenerGuid, new ListenerActionContainer(listener, action));
        }

        public void Invoke(T data)
        {
            foreach (var listener in listeners)
            {
                var actualListener = listener.Value;

                if (actualListener.Listener == null || !actualListener.Listener.Owner.IsAlive)
                {
                    listenersToRemove.Enqueue(listener.Value.Guid);
                    continue;
                }

                if (actualListener.Listener.Owner.IsPaused)
                    continue;

                actualListener.Action(data);
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

    public interface IRemoveSystemListener
    {
        void RemoveListener(ISystem listener);
    }

    public interface ICommandService : IDisposable
    {
        void AddListener<T>(ISystem listener, Action<T> action);
        void Invoke<T>(T data);
        void RemoveListener<T>(ISystem listener);
        void ReleaseListener(ISystem listener);
    }
}
