using System;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, "this interface provide subscribe to event  - add or remove any component by interface in world")]
    public interface IReactGenericGlobalComponent<T> : IHaveOwner
    {
        Guid ListenerGuid { get; }
        void ComponentReact(T component, bool isAdded);
    }

    [Documentation(Doc.HECS, "this interface provide subscribe to event  - add or remove any component by interface on local entity")]
    public interface IReactGenericLocalComponent<T> : IHaveOwner
    {
        Guid ListenerGuid { get; }
        void ComponentReactLocal(T component, bool isAdded);
    }

    [Documentation(Doc.HECS, "the implementation of this interface in the system allows you to find out the add or removal of a component of a particular type on the current entity")]
    public interface IReactComponentLocal<T> : IHaveOwner where T : IComponent  
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ComponentReact(T component, bool isAdded);
    }

    [Documentation(Doc.HECS, "the implementation of this interface in the system allows you react to adding or removal of any component of a particular type on the current world")]
    public interface IReactComponentGlobal<T> : IHaveOwner where T : IComponent
    {
        void ComponentReactGlobal(T component, bool isAdded);
    }
}