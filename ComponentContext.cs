using System;

namespace HECSFramework.Core 
{
    public partial class ComponentContext
    {
        public void RemoveComponent(IComponent component) => Remove(component);
        public void AddComponent(IComponent component) => Add(component);
        public Action DisposeContext;

        partial void Remove(IComponent component);
        partial void Add(IComponent component);
    }
}