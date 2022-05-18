using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, "Это новая реализация, тут мы подписываемся на конкретный глобальный компонент, это происходит через реализацию в системе IReactComponentGlobal<T>")]
    public sealed class GlobalComponentListenersService : IDisposable
    {
        private Dictionary<Type, object> componentListeners = new Dictionary<Type, object>();

        public void Invoke<T>(T componentType, bool isAdded) where T : IComponent
        {
            var key = typeof(T);
            if (!componentListeners.TryGetValue(key, out var commandListenerContainer))
                return;
            var eventContainer = (GlobalComponentsListenerContainer<T>)commandListenerContainer;
            eventContainer.Invoke(componentType, isAdded);
        }

        public void ReleaseListener(ISystem listener)
        {
            foreach (var l in componentListeners)
                (l.Value as IRemoveSystemListener).RemoveListener(listener);
        }

        public void RemoveListener<T>(ISystem listener) where T : IComponent
        {
            var key = typeof(T);
            if (componentListeners.TryGetValue(key, out var container))
            {
                var eventContainer = (GlobalComponentsListenerContainer<T>)container;
                eventContainer.RemoveListener(listener);
            }
        }

        public void AddListener<T>(ISystem listener, IReactComponentGlobal<T> action) where T : IComponent
        {
            var key = typeof(T);

            if (componentListeners.ContainsKey(key))
            {
                var lr = (GlobalComponentsListenerContainer<T>)componentListeners[key];
                lr.ListenCommand(listener, action);
                return;
            }
            componentListeners.Add(key, new GlobalComponentsListenerContainer<T>(listener, action));
        }

        public void Dispose()
        {
            foreach (var t in componentListeners.Values)
            {
                if (t is IDisposable disposable)
                    disposable.Dispose();
            }

            componentListeners.Clear();
        }
    }
}