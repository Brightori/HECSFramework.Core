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
        private sealed class ListenerActionContainer
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

        private List<ListenerActionContainer> listeners =
            new List<ListenerActionContainer>(8);

        private Queue<Guid> listenersToRemove = new Queue<Guid>(4);

        public CommandListener(ISystem listener, Action<T> action)
        {
            listeners.Add(new ListenerActionContainer(listener, action));
        }

        public void ListenCommand(ISystem listener, Action<T> action)
        {
            if (listeners.Any(x => x.Guid == listener.SystemGuid))
                return;

            listeners.Add(new ListenerActionContainer(listener, action));
        }

        public void Invoke(T data)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                if (listeners[i].Listener == null || !listeners[i].Listener.Owner.IsAlive)
                {
                    listenersToRemove.Enqueue(listeners[i].Guid);
                    continue;
                }

                if (listeners[i].Listener.Owner.IsPaused)
                    continue;

                listeners[i].Action(data);
            }

            while (listenersToRemove.Count > 0)
            {
                var remove = listenersToRemove.Dequeue();

                for (int i = 0; i < listeners.Count; i++)
                {
                    if (listeners[i].Guid == remove)
                    {
                        listeners.RemoveAt(i);
                        break;
                    }
                }   
            }
        }

        public void RemoveListener(ISystem listener)
        {
            var needed = listeners.FirstOrDefault(x => x.Guid == listener.SystemGuid);

            if (needed != null)
                listeners.Remove(needed);
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
