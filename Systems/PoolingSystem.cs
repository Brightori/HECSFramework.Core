using HECSFramework.Core;
using System;
using System.Collections.Generic;

namespace Systems
{
    public partial class PoolingSystem : BaseSystem, IReactComponent
    {
        private int maxCapacity = 512;

        private Dictionary<int, Stack<IComponent>> poolOfComponents = new Dictionary<int, Stack<IComponent>>(8);
        public System.Guid ListenerGuid { get; } = Guid.NewGuid();
        
        public override void InitSystem()
        {
        }

        public T GetComponentFromPool<T>(ref HECSMask mask) where T : IComponent
        {
            var index = mask.Index;

            if (poolOfComponents.ContainsKey(index))
            {
                if (poolOfComponents[index].Count > 0)
                    return (T)poolOfComponents[index].Pop();
            }
            else
                poolOfComponents.Add(index, new Stack<IComponent>());

            return (T)TypesMap.GetComponentFromFactory(mask.TypeHashCode);
        }

        public void ComponentReact(IComponent component, bool isAdded)
        {
            if (!isAdded)
            {
                var index = component.ComponentsMask.Index;
                
                if (poolOfComponents.ContainsKey(index) && poolOfComponents[index].Count < maxCapacity)
                    poolOfComponents[index].Push(component);
            }
        }
    }
}