using System;
using System.Collections.Generic;
using System.Reflection;

namespace HECSFramework.Core
{
    /// <summary>
    /// Рукописное ядро регистрации типов. Кодоген больше не генерирует конструктор
    /// TypesProvider — вместо этого каждый тип приносит свой контейнер через метод
    /// Register_<TypeName>() в partial-части этого класса (файл на тип в Containers/).
    /// Методы собираются рефлексией по префиксу; всё глобальное (индексы, маски,
    /// словари) вычисляется в Build() по порядку регистрации.
    /// Префикс "Register_" зарезервирован: рукописные методы не должны его использовать.
    /// </summary>
    public partial class TypesProvider : IHECSFactory
    {
        public const string RegistrationMethodPrefix = "Register_";

        private readonly List<IComponentContainer> componentContainers = new List<IComponentContainer>(512);
        private readonly List<ISystemContainer> systemContainers = new List<ISystemContainer>(256);

        private readonly Dictionary<int, IComponentContainer> componentsByHash = new Dictionary<int, IComponentContainer>(512);
        private readonly Dictionary<int, ISystemContainer> systemsByHash = new Dictionary<int, ISystemContainer>(256);

        public TypesProvider()
        {
            CollectRegistrations();
            Build();
        }

        public void RegisterComponent(IComponentContainer container) => componentContainers.Add(container);
        public void RegisterSystem(ISystemContainer container) => systemContainers.Add(container);

        private void CollectRegistrations()
        {
            var methods = typeof(TypesProvider).GetMethods(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                if (!method.Name.StartsWith(RegistrationMethodPrefix, StringComparison.Ordinal))
                    continue;

                if (method.GetParameters().Length != 0 || method.ReturnType != typeof(void))
                {
                    HECSDebug.LogWarning($"TypesProvider: метод {method.Name} совпал с префиксом регистрации, но имеет неподходящую сигнатуру, пропущен");
                    continue;
                }

                method.Invoke(this, null);
            }
        }

        /// <summary>
        /// Единственное место в системе, где существуют «индекс» и «бит маски».
        /// Индексы раздаются по порядку регистрации — порядок процессно-локален,
        /// наружу (сеть/сейвы) уходят только TypeHashCode и ShortID.
        /// </summary>
        private void Build()
        {
            Count = componentContainers.Count + 1;

            MapIndexes = new Dictionary<int, ComponentMaskAndIndex>(Count)
            {
                { -1, new ComponentMaskAndIndex { ComponentName = "DefaultEmpty", ComponentsMask = HECSMask.Empty } }
            };

            TypeToComponentIndex = new Dictionary<Type, int>(Count);
            HashToType = new Dictionary<int, Type>(Count);
            TypeToHash = new Dictionary<Type, int>(Count);

            for (int i = 0; i < componentContainers.Count; i++)
            {
                var container = componentContainers[i];

                //индекс типа совпадает с индексом маски, как в монолитном TypesProvider: 0 занят DefaultEmpty
                var index = i + 1;
                var mask = new HECSMask { Index = index, TypeHashCode = container.TypeHashCode };

                MapIndexes.Add(container.TypeHashCode, new ComponentMaskAndIndex
                {
                    ComponentName = container.ComponentType.Name,
                    ComponentsMask = mask,
                });

                TypeToComponentIndex.Add(container.ComponentType, index);
                TypeToHash.Add(container.ComponentType, container.TypeHashCode);
                HashToType.Add(container.TypeHashCode, container.ComponentType);
                componentsByHash.Add(container.TypeHashCode, container);

                container.AfterBuild(mask, index);
            }

            foreach (var system in systemContainers)
                systemsByHash.Add(system.TypeHashCode, system);

            HECSFactory = this;
        }

        /// <summary>
        /// Контейнеры систем в виде словаря для TypesMap (контейнер и есть ISystemSetter).
        /// </summary>
        public Dictionary<Type, ISystemSetter> GetSystemContainers()
        {
            var setters = new Dictionary<Type, ISystemSetter>(systemContainers.Count);

            foreach (var system in systemContainers)
                setters.Add(system.SystemType, system);

            return setters;
        }

        #region IHECSFactory
        public IComponent GetComponentFromFactory(int hashCodeType) => componentsByHash[hashCodeType].Factory();

        public T GetComponentFromFactory<T>() where T : class, IComponent
            => (T)componentsByHash[TypeToHash[typeof(T)]].Factory();

        public ISystem GetSystemFromFactory(int hashCodeType) => systemsByHash[hashCodeType].Factory();
        #endregion
    }
}
