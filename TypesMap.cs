using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public static partial class TypesMap
    {
        public static readonly int SizeOfComponents = 64;

        private static readonly Dictionary<int, ComponentMaskAndIndex> MapIndexes;
        private static readonly Dictionary<Type, int> TypeToComponentIndex;
        private static readonly Dictionary<Type, int> TypeToHash;
        private static readonly Dictionary<int, Type> componentHashToType;
        private static Dictionary<int, IComponentContextSetter> componentsSetters;
        private static Dictionary<Type, ISystemSetter> systemsSetters;
        private static IHECSFactory factory;
        private static readonly IReadOnlyList<ITypeContainer> containers;

        public static IReadOnlyList<IComponentContainer> ComponentContainers { get; }
        public static IReadOnlyList<IFastComponentContainer> FastComponentContainers { get; }
        public static IReadOnlyDictionary<int, ComponentMaskAndIndex> ComponentsInfo => MapIndexes;
        public static IReadOnlyCollection<Type> ComponentTypes => TypeToComponentIndex.Keys;
        public static IReadOnlyCollection<Type> SystemTypes => systemsSetters.Keys;

        static TypesMap()
        {
            var typeProvider = new TypesProvider();
            MapIndexes = typeProvider.MapIndexes;
            TypeToComponentIndex = typeProvider.TypeToComponentIndex;
            SizeOfComponents = typeProvider.Count;
            factory = typeProvider.HECSFactory;
            TypeToHash = typeProvider.TypeToHash;
            componentHashToType = typeProvider.HashToType;
            containers = typeProvider.Containers;
            ComponentContainers = typeProvider.ComponentContainers;
            FastComponentContainers = typeProvider.FastComponentContainers;

            //биндинги систем приходят из контейнеров, собранных TypesProvider
            systemsSetters = typeProvider.GetSystemContainers();
        }

        public static IEnumerable<T> GetContainers<T>() where T : ITypeContainer
        {
            foreach (var container in containers)
            {
                if (container is T needed)
                    yield return needed;
            }
        }

        public static void BindSystem<T>(in T system) where T: ISystem
        {
            var key = system.GetType();
            systemsSetters[key].BindSystem(system);
        }

        public static bool ContainsComponent(int index)
        {
            return componentHashToType.ContainsKey(index);
        }

        public static void UnBindSystem<T>(T system) where T : ISystem
        {
            var key = system.GetType();
            systemsSetters[key].UnBindSystem(system);
        }

        public static void SetComponent(Entity entity, IComponent component)
        {
            componentsSetters[component.GetTypeHashCode].SetComponent(entity, component);
        }

        public static void RemoveComponent(Entity entity, IComponent component)
        {
            componentsSetters[component.GetTypeHashCode].RemoveComponent(entity, component);
        }

        public static void RegisterComponent(int index, Entity entity, bool isAdded)
        {
            componentsSetters[index].RegisterComponent(entity, isAdded);
        }

        public static int GetHashOfComponentByType(Type type)
        {
            return TypeToHash[type];
        } 
        
        public static int GetHashOfComponentByType<T>()
        {
            return TypeToHash[typeof(T)];
        }

        public static bool GetComponentInfo(int hashTypeCode, out ComponentMaskAndIndex mask)
        {
            return MapIndexes.TryGetValue(hashTypeCode, out mask);
        }      
        
        public static ComponentMaskAndIndex GetComponentInfo<T>() where T: IComponent
        {
            var hashTypeCode = GetHashOfComponentByType<T>();
            return MapIndexes[hashTypeCode];
        }

        public static ComponentMaskAndIndex GetComponentInfo(IComponent component) 
        {
            try
            {
                var hashTypeCode = TypeToHash[component.GetType()];
                return MapIndexes[hashTypeCode];
            }
            catch
            {
                throw new InvalidOperationException();
            }
        }
       

        public static Type GetTypeByComponentHECSHash(int hash)
        {
            return componentHashToType[hash];
        }

        public static int GetIndexByType<T>() where T : IComponent
        {
            return TypeToComponentIndex[typeof(T)];
        }

        public static IComponent GetComponentFromFactory(int hashCodeType)
        {
            return factory.GetComponentFromFactory(hashCodeType);
        }

        public static ISystem GetSystemFromFactory(int hashCodeType)
        {
            return factory.GetSystemFromFactory(hashCodeType);
        }

        public static T GetComponentFromFactory<T>() where T : class, IComponent
        {
            return factory.GetComponentFromFactory<T>();
        }
    }

    public interface IComponentContextSetter
    {
        void SetComponent(Entity entity, IComponent component);
        void RemoveComponent(Entity entity, IComponent component);
        void RegisterComponent(Entity entity, bool isAdded);
    }

    public interface ISystemSetter 
    {
        void BindSystem(ISystem system);
        void UnBindSystem(ISystem system);
    }
}