using System;

namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, "реализация этого интерфейса в системе, позволяет узнать появление или удаление любого компонента в рамках мира")]
    public interface IReactComponent 
    {
        Guid ListenerGuid { get; }
        void ComponentReact(IComponent component, bool isAdded);
    }

    [Documentation(Doc.HECS, "реализация этого интерфейса в системе, позволяет узнать появление или удаление компонента конкретного типа на текущей ентити")]
    public interface IReactComponentLocal<T>  where T : IComponent
    {
        Guid ListenerGuid { get; }
        void ComponentReact(T component, bool isAdded);
    }

    [Documentation(Doc.HECS, "реализация этого интерфейса в системе, позволяет узнать появление или удаление компонента конкретного типа в мире")]
    public interface IReactComponentGlobal<T> where T : IComponent
    {
        Guid ListenerGuid { get; }
        void ComponentReact(T component, bool isAdded);
    }
}