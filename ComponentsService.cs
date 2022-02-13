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

        public void RemoveListener<T>(T listener) where T: ISystem
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

        public void AddListener(IReactComponent listener)
        {
            if (listeners.ContainsKey(listener.ListenerGuid))
                return;

            listeners.Add(listener.ListenerGuid, listener.ComponentReact);
        }

        public void RemoveListener(IReactComponent listener)
        {
            if (listeners.TryGetValue(listener.ListenerGuid, out var action))
            {
                listeners.Remove(listener.ListenerGuid);
            }
        }

        public void ProcessComponent(IComponent component, bool isAdded)
        {
            foreach (var listener in listeners.Values)
                listener.Invoke(component, isAdded);
        }
        
        public void Dispose()
        {
            listeners.Clear();
        }
    }

    public sealed class Listener<T,U> : IRemoveSystemListener 
    {
        private struct TwoArgsListenContainer 
        {
            public readonly Action<T,U> Action;
            public readonly ISystem Listener;

            public TwoArgsListenContainer(ISystem listener, Action<T,U> action)
            {
                Listener = listener;
                Action = action;
            }

            public override bool Equals(object obj)
            {
                return obj is TwoArgsListenContainer container &&
                       Listener.SystemGuid.Equals(container.Listener.SystemGuid);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Listener.SystemGuid);
            }
        }

       private Dictionary<Guid, TwoArgsListenContainer> listeners = new Dictionary<Guid, TwoArgsListenContainer>(16);

        private Queue<Guid> listenersToRemove = new Queue<Guid>(4);

        public Listener(ISystem listener, Action<T,U> action)
        {
            listeners.Add(listener.SystemGuid, new TwoArgsListenContainer(listener, action));
        }

        public void ListenCommand(ISystem listener, Action<T,U> action)
        {
            if (listeners.ContainsKey(listener.SystemGuid))
                return;

            listeners.Add(listener.SystemGuid, new TwoArgsListenContainer(listener, action));
        }

        public void Invoke(T arg, U arg2)
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

                actualListener.Action(arg, arg2);
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