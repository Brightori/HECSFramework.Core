using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, "Это новая реализация, тут мы подписываемся на конкретный компонент, этот сервис может быть как локально так и глобально")]
    public sealed class RegisterComponentListenersService : IDisposable
    {
        private Dictionary<Type, object> componentListeners = new Dictionary<Type, object>();

        public void Invoke<T>(T componentType, bool isAdded)
        {
            var key = typeof(T);
            if (!componentListeners.TryGetValue(key, out var commandListenerContainer))
                return;
            var eventContainer = (Listener<T, bool>)commandListenerContainer;
            eventContainer.Invoke(componentType, isAdded);
        }

        public void ReleaseListener(ISystem listener)
        {
            foreach (var l in componentListeners)
                (l.Value as IRemoveSystemListener).RemoveListener(listener);
        }

        public void RemoveListener<T>(ISystem listener)
        {
            var key = typeof(T);
            if (componentListeners.TryGetValue(key, out var container))
            {
                var eventContainer = (Listener<T,bool>)container;
                eventContainer.RemoveListener(listener);
            }
        }

        public void AddListener<T>(ISystem listener, Action<T, bool> action)
        {
            var key = typeof(T);

            if (componentListeners.ContainsKey(key))
            {
                var lr = (Listener<T, bool>)componentListeners[key];
                lr.ListenCommand(listener, action);
                return;
            }
            componentListeners.Add(key, new Listener<T,bool>(listener, action));
        }

        public void Dispose()
        {
            componentListeners.Clear();
        }
    }

    [Documentation (Doc.GameLogic, Doc.HECS, "Это старая реализация, где мы глобально подписываемся на все компоненты подряд")]
    public sealed partial class ComponentsService : IDisposable
    {
        private Dictionary<Guid, Action<IComponent, bool>> listeners = new Dictionary<Guid, Action<IComponent, bool>>(16);
        private List<Action<IComponent, bool>> reactions = new List<Action<IComponent, bool>>(16);

        public void AddListener(IReactComponent listener)
        {
            if (listeners.ContainsKey(listener.ListenerGuid))
                return;

            listeners.Add(listener.ListenerGuid, listener.ComponentReact);
            reactions.Add(listener.ComponentReact);
        }

        public void RemoveListener(IReactComponent listener)
        {
            if (listeners.TryGetValue(listener.ListenerGuid, out var action))
            {
                listeners.Remove(listener.ListenerGuid);
                reactions.Remove(listener.ComponentReact);
            }
        }

        public void ProcessComponent(IComponent component, bool isAdded)
        {
            var count = reactions.Count;

            for (int i = 0; i < count; i++)
                reactions[i].Invoke(component, isAdded);
        }
        
        public void Dispose()
        {
            listeners.Clear();
        }
    }

    public sealed class Listener<T,U> : IRemoveSystemListener 
    {
        private sealed class TwoArgsListenContainer 
        {
            public readonly Action<T,U> Action;
            public readonly ISystem Listener;
            public Guid Guid;

            public TwoArgsListenContainer(ISystem listener, Action<T,U> action)
            {
                Listener = listener;
                Action = action;
                Guid = listener.SystemGuid;
            }

            public override bool Equals(object obj)
            {
                return obj is TwoArgsListenContainer container &&
                       Guid.Equals(container.Guid);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Guid);
            }
        }

        private List<TwoArgsListenContainer> listeners =
            new List<TwoArgsListenContainer>(8);

        private Queue<Guid> listenersToRemove = new Queue<Guid>(4);

        public Listener(ISystem listener, Action<T,U> action)
        {
            listeners.Add(new TwoArgsListenContainer(listener, action));
        }

        public void ListenCommand(ISystem listener, Action<T,U> action)
        {
            if (listeners.Any(x => x.Guid == listener.SystemGuid))
                return;

            listeners.Add(new TwoArgsListenContainer(listener, action));
        }

        public void Invoke(T arg, U arg2)
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

                listeners[i].Action(arg,arg2);
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
}