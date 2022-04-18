using System;

namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, "реализация этого интерфейса в системе, позволяет узнать появление или удаление любого компонента в рамках мира")]
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

    [Documentation(Doc.HECS, "реализация этого интерфейса в системе, позволяет узнать появление или удаление компонента конкретного типа в мире")]
    public interface IReactComponentGlobal<T> where T : IComponent
    {
        void ComponentReactGlobal(T component, bool isAdded);
    }
}