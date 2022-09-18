using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class EntityLocalCommandService
    {
        private Dictionary<Type, object> commandListeners = new Dictionary<Type, object>();

        public void Invoke<T>(T data) where T : struct, ICommand
        {
            var key = typeof(T);
            if (!commandListeners.TryGetValue(key, out var commandListenerContainer))
                return;
            var eventContainer = (LocalCommandListener<T>)commandListenerContainer;
            eventContainer.Invoke(data);
        }

        public void ReleaseListener(ISystem listener)
        {
            foreach (var l in commandListeners)
                (l.Value as IRemoveSystemListener).RemoveListener(listener);
        }

        public void RemoveListener<T>(ISystem listener) where T : struct, ICommand
        {
            var key = typeof(T);
            if (commandListeners.TryGetValue(key, out var container))
            {
                var eventContainer = (LocalCommandListener<T>)container;
                eventContainer.RemoveListener(listener);
            }
        }

        public void AddListener<T>(ISystem listener, IReactCommand<T> action) where T : struct, ICommand
        {
            var key = typeof(T);

            if (commandListeners.ContainsKey(key))
            {
                var lr = (LocalCommandListener<T>)commandListeners[key];
                lr.ListenCommand(listener, action);
                return;
            }
            commandListeners.Add(key, new LocalCommandListener<T>(listener, action));
        }

        public void Dispose()
        {
            commandListeners.Clear();
        }
    }
}

namespace HECSFramework.Core
{
    public sealed class LocalCommandListener<T> : IRemoveSystemListener where T : struct, ICommand
    {
        private struct ListenerContainer
        {
            public IEntity Owner;
            public IReactCommand<T> Listener;

            public ListenerContainer(IEntity owner, IReactCommand<T> listener)
            {
                Owner = owner;
                Listener = listener;
            }
        }

        private bool inProgress = false;

        private Dictionary<Guid, ListenerContainer> listeners = new Dictionary<Guid, ListenerContainer>(16);
        private Queue<Guid> listenersToRemove = new Queue<Guid>(4);
        private bool isDirty;

        public LocalCommandListener(ISystem listener, IReactCommand<T> react)
        {
            listeners.Add(listener.SystemGuid, new ListenerContainer(listener.Owner, react));
        }

        public void ListenCommand(ISystem listener, IReactCommand<T> react)
        {
            var listenerGuid = listener.SystemGuid;

            if (listeners.ContainsKey(listenerGuid))
                return;

            listeners.Add(listenerGuid, new ListenerContainer(listener.Owner, react));
        }

        public void Invoke(T data)
        {
            ProcessRemove();

            inProgress = true;
            foreach (var listener in listeners)
            {
                var actualListener = listener.Value;

                if (actualListener.Owner == null || !actualListener.Owner.IsAlive)
                {
                    listenersToRemove.Enqueue(listener.Key);
                    isDirty = true;
                    continue;
                }

                if (actualListener.Owner.IsPaused)
                    continue;

                actualListener.Listener.CommandReact(data);
            }
            inProgress = false;
            ProcessRemove();
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
                listenersToRemove.Enqueue(listener.SystemGuid);
            
            isDirty = true;

            if (!inProgress)
                ProcessRemove();
        }
    }

    public interface IRemoveSystemListener
    {
        void RemoveListener(ISystem listener);
    }
}
