using System;
using HECSFramework.Core;

namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, "this interface provide subscribe to event  - add or remove any component in world")]
    public interface IReactComponent 
    {
        Guid ListenerGuid { get; }
        void ComponentReact<T>(T component, bool isAdded) where T: IComponent;
    }

    [Documentation(Doc.HECS, "this interface provide subscribe to event  - add or remove any component on local entity")]
    public interface IReactComponentLocal
    {
        void ComponentReactLocal(IComponent component, bool isAdded);
    }

    [Documentation(Doc.HECS, "the implementation of this interface in the system allows you to find out the add or removal of a component of a particular type on the current entity")]
    public interface IReactComponentLocal<T> where T : IComponent  
    {
        void ComponentReact(T component, bool isAdded);
    }

    [Documentation(Doc.HECS, "the implementation of this interface in the system allows you react to adding or removal of any component of a particular type on the current world")]
    public interface IReactComponentGlobal<T> where T : IComponent
    {
        void ComponentReactGlobal(T component, bool isAdded);
    }
}