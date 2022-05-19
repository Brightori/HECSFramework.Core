using System;
using HECSFramework.Core;

namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, "this interface provide subscribe to event  - add or remove any component in world")]
    public interface IReactComponent 
    {
        Guid ListenerGuid { get; }
        void ComponentReact(IComponent component, bool isAdded);
    }

    [Documentation(Doc.HECS, "the implementation of this interface in the system allows you to find out the add or removal of a component of a particular type on the current entity")]
    public interface IReactComponentLocal<T>  
    {
        void ComponentReact(T component, bool isAdded);
    }

    [Documentation(Doc.HECS, "the implementation of this interface in the system allows you to find out the add or removal of a component of a particular type on the current world")]
    public interface IReactComponentGlobal<T> where T : IComponent
    {
        void ComponentReactGlobal(T component, bool isAdded);
    }
}

namespace Commands
{
    [Documentation(Doc.HECS, "This command send when we need know when component added localy and just need any type of component for checking, this is case for counters or any other interfaces beyond the components, its pretty rare case and bcz we use for this command and not use a special service like in other cases")]
    public struct LocalComponentAddedEvent : ICommand
    {
        public bool IsAdded;
        public IComponent Component;
    }
}