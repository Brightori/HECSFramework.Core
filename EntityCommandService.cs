using HECSFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core 
{
    public class EntityCommandService : ICommandService
    {
        private Dictionary<Type, object> commandListeners = new Dictionary<Type, object>();

        public void Invoke<T>(T data) where T : ICommand
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

        public void RemoveListener<T>(ISystem listener) where T : ICommand
        {
            var key = typeof(T);
            if (commandListeners.TryGetValue(key, out var container))
            {
                var eventContainer = (CommandListener<T>)container;
                eventContainer.RemoveListener(listener);
            }
        }

        public void AddListener<T>(ISystem listener, Action<T> action) where T : ICommand
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
    public class CommandListener<T> : IRemoveSystemListener where T : ICommand
    {
        private List<(ISystem listener, Action<T> react)> listeners =
            new List<(ISystem listener, Action<T> react)>(8);

        private Queue<(ISystem listener, Action<T> react)> listenersToRemove = new Queue<(ISystem listener, Action<T> react)>(4);

        public CommandListener(ISystem listener, Action<T> action)
        {
            listeners.Add((listener, action));
        }

        public void ListenCommand(ISystem listener, Action<T> action)
        {
            if (listeners.Any(x => x.listener == listener))
                return;

            listeners.Add((listener, action));
        }

        public void Invoke(T data)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                if (listeners[i].listener.Owner == null || !listeners[i].listener.Owner.IsAlive)
                {
                    listenersToRemove.Enqueue(listeners[i]);
                    continue;
                }

                if (listeners[i].listener.Owner.IsPaused)
                    continue;

                listeners[i].react(data);
            }

            while (listenersToRemove.Count > 0)
            {
                listeners.Remove(listenersToRemove.Dequeue());
            }
        }

        public void RemoveListener(ISystem listener)
        {
            var needed = listeners.FirstOrDefault(x => x.listener == listener);

            if (needed.listener != null)
                listeners.Remove(needed);
        }
    }

    public interface IRemoveSystemListener
    {
        void RemoveListener(ISystem listener);
    }

    public interface ICommandService : IDisposable
    {
        void AddListener<T>(ISystem listener, Action<T> action) where T : ICommand;
        void Invoke<T>(T data) where T : ICommand;
        void RemoveListener<T>(ISystem listener) where T : ICommand;
        void ReleaseListener(ISystem listener);
    }
}
