using System;

namespace HECSFramework.Core
{
    /// <summary>
    /// Изолированный контейнер компонента: несёт только локальные данные типа.
    /// Генерируется кодогеном в отдельный файл на тип (Containers/<X>.Container.cs).
    /// Индекс и маска назначаются в TypesProvider.Build() и приходят через AfterBuild.
    /// </summary>
    public interface IComponentContainer
    {
        Type ComponentType { get; }
        int TypeHashCode { get; }
        IComponent Factory();

        /// <summary>
        /// Обратная связь после Build(): контейнер узнаёт назначенные index и mask.
        /// </summary>
        void AfterBuild(HECSMask mask, int index);
    }

    /// <summary>
    /// Изолированный контейнер системы. Расширяет уже существующий ISystemSetter
    /// (BindSystem/UnBindSystem), тела биндингов генерируются кодогеном.
    /// </summary>
    public interface ISystemContainer : ISystemSetter
    {
        Type SystemType { get; }
        int TypeHashCode { get; }
        ISystem Factory();
    }

    /// <summary>
    /// Локальный Preserve-атрибут: Unity linker сохраняет члены с атрибутом,
    /// имя которого — PreserveAttribute, независимо от неймспейса.
    /// Нужен, потому что методы Register_* вызываются только рефлексией.
    /// На сервере (без Unity) — просто безвредный атрибут.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class PreserveAttribute : Attribute
    {
    }
}
