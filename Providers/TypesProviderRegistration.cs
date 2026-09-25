using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    /// <summary>
    /// Рукописное ядро регистрации типов. Каждый тип приносит свой контейнер строкой
    /// в TypeContainersRegistry (файл на тип в Containers/); всё глобальное
    /// (индексы, маски, словари) вычисляется в Build().
    /// </summary>
    public partial class TypesProvider : IHECSFactory
    {
        private readonly List<IComponentContainer> componentContainers = new List<IComponentContainer>(512);
        private readonly List<ISystemContainer> systemContainers = new List<ISystemContainer>(256);
        private readonly List<IFastComponentContainer> fastComponentContainers = new List<IFastComponentContainer>(16);

        private readonly Dictionary<int, IComponentContainer> componentsByHash = new Dictionary<int, IComponentContainer>(512);
        private readonly Dictionary<int, ISystemContainer> systemsByHash = new Dictionary<int, ISystemContainer>(256);

        public IReadOnlyList<ITypeContainer> Containers => TypeContainersRegistry.All;
        public IReadOnlyList<IComponentContainer> ComponentContainers => componentContainers;
        public IReadOnlyList<ISystemContainer> SystemContainers => systemContainers;
        public IReadOnlyList<IFastComponentContainer> FastComponentContainers => fastComponentContainers;

        public TypesProvider()
        {
            foreach (var container in TypeContainersRegistry.All)
            {
                if (container is IComponentContainer component)
                    componentContainers.Add(component);

                if (container is ISystemContainer system)
                    systemContainers.Add(system);

                if (container is IFastComponentContainer fastComponent)
                    fastComponentContainers.Add(fastComponent);
            }

            Build();
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

                if (componentsByHash.TryGetValue(container.TypeHashCode, out var registered))
                    throw SameTypeHashCode(registered.ComponentType, container.ComponentType, container.TypeHashCode);

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
            }

            foreach (var system in systemContainers)
            {
                if (systemsByHash.TryGetValue(system.TypeHashCode, out var registered))
                    throw SameTypeHashCode(registered.SystemType, system.SystemType, system.TypeHashCode);

                systemsByHash.Add(system.TypeHashCode, system);
            }

            HECSFactory = this;
        }

        private static InvalidOperationException SameTypeHashCode(Type registered, Type added, int hash)
            => new InvalidOperationException($"TypesProvider: {registered.FullName} and {added.FullName} have the same TypeHashCode {hash}, rename one of them");

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
