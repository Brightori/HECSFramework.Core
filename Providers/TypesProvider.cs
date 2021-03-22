using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public partial class TypesProvider
    {
        public int Count { get; private set; }
        public  Dictionary<int, ComponentMaskAndIndex> MapIndexes { get; private set; }
        public  Dictionary<Type, int> TypeToComponentIndex { get; private set; }

        public IComponentFactory ComponentFactory { get; private set; }
    }

    public interface IComponentFactory
    {
        IComponent GetComponentFromFactory(int hashCodeType);
        IComponent GetComponentFromFactory<T>() where T : class, IComponent;
    }
}