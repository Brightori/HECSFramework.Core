using System;

namespace HECSFramework.Core
{
    public interface ITypeContainer
    {
    }

    /// <summary>
    /// Изолированный контейнер компонента: несёт только локальные данные типа.
    /// Генерируется кодогеном в отдельный файл на тип (Containers/<X>Container.cs).
    /// </summary>
    public interface IComponentContainer : ITypeContainer
    {
        Type ComponentType { get; }
        int TypeHashCode { get; }
        IComponent Factory();
        void RegisterWorld(World world);
    }

    /// <summary>
    /// Изолированный контейнер системы. Расширяет уже существующий ISystemSetter
    /// (BindSystem/UnBindSystem), тела биндингов генерируются кодогеном.
    /// </summary>
    public interface ISystemContainer : ISystemSetter, ITypeContainer
    {
        Type SystemType { get; }
        int TypeHashCode { get; }
        ISystem Factory();
    }

    public interface IFastComponentContainer : ITypeContainer
    {
        void RegisterWorld(World world);
        void UnRegisterWorld(World world);
    }
}
