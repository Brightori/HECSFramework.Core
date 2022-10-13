using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, "Это новая реализация, тут мы подписываемся на конкретный локальный компонент, это происходит через реализацию в системе IReactComponentLocal<T>")]
    public sealed class LocalComponentListenersService : IDisposable
    {
        private Dictionary<Type, object> componentListeners = new Dictionary<Type, object>();
        private LocalComponentsListenerContainer localComponentsListenerContainer = new LocalComponentsListenerContainer();

        public void Invoke<T>(T component, bool isAdded) where T: IComponent
        {
            localComponentsListenerContainer.Invoke(component, isAdded);

            var key = typeof(T);

            if (!componentListeners.TryGetValue(key, out var commandListenerContainer))
                return;
            
            var eventContainer = (LocalComponentsListenerContainer<T>)commandListenerContainer;
            eventContainer.Invoke(component, isAdded);
        }

        public void ReleaseListener(ISystem listener)
        {
            foreach (var l in componentListeners)
                (l.Value as IRemoveSystemListener).RemoveListener(listener);

            localComponentsListenerContainer.RemoveListener(listener);
        }

        public void RemoveListener<T>(ISystem listener) where T : IComponent
        {
            var key = typeof(T);
            if (componentListeners.TryGetValue(key, out var container))
            {
                var eventContainer = (LocalComponentsListenerContainer<T>)container;
                eventContainer.RemoveListener(listener);
            }

            if (listener is IReactComponentLocal reactComponentLocal)
                localComponentsListenerContainer.RemoveListener(listener);
        }

        public void AddLocalListener(ISystem listener, IReactComponentLocal action)
        {
            localComponentsListenerContainer.ListenCommand(listener, action);
        }

        public void AddListener<T>(ISystem listener, IReactComponentLocal<T> action) where T : IComponent
        {
            var key = typeof(T);

            if (componentListeners.ContainsKey(key))
            {
                var lr = (LocalComponentsListenerContainer<T>)componentListeners[key];
                lr.ListenCommand(listener, action);
                return;
            }
            componentListeners.Add(key, new LocalComponentsListenerContainer<T>(listener, action));
        }

        public void Dispose()
        {
            foreach (var c in componentListeners.Values)
            {
                if (c is IDisposable disposable)
                    disposable.Dispose();
            }

            componentListeners.Clear();
        }
    }
}