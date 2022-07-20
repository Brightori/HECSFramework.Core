using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{

    [Documentation (Doc.GameLogic, Doc.HECS, "Это старая реализация, где мы глобально подписываемся на все компоненты подряд")]
    public sealed partial class ComponentsService : IDisposable
    {
        private Dictionary<Guid, IReactComponent> listeners = new Dictionary<Guid, IReactComponent>(16);

        public void AddListener(IReactComponent listener)
        {
            if (listeners.ContainsKey(listener.ListenerGuid))
                return;

            listeners.Add(listener.ListenerGuid, listener);
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
            if (component == null)
                return;

            foreach (var listener in listeners.Values)
                listener.ComponentReact(component, isAdded);
        }
        
        public void Dispose()
        {
            listeners.Clear();
        }
    }
}