using System;

namespace HECSFramework.Core
{
    public interface IReactComponent 
    {
        Guid ListenerGuid { get; }
        void ComponentReact(IComponent component, bool isAdded);
    }
}