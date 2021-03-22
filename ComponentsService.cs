using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public partial class ComponentsService : IDisposable
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
}