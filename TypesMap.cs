using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public static partial class TypesMap
    {
        public static readonly int SizeOfComponents = 64;
        public static IMaskProvider MaskProvider;

        private static readonly Dictionary<int, ComponentMaskAndIndex> MapIndexes;
        private static readonly Dictionary<Type, int> TypeToComponentIndex;
        private static readonly Dictionary<Type, int> TypeToHash;
        private static readonly Dictionary<int, Type> componentHashToType;
        private static Dictionary<int, IComponentContextSetter> componentsSetters;
        private static Dictionary<Type, ISystemSetter> systemsSetters;
        private static IHECSFactory factory;

        static TypesMap()
        {
            MaskProvider = new MaskProvider();
            var typeProvider = new TypesProvider();
            MapIndexes = typeProvider.MapIndexes;
            TypeToComponentIndex = typeProvider.TypeToComponentIndex;
            SizeOfComponents = typeProvider.Count;
            factory = typeProvider.HECSFactory;
            TypeToHash = typeProvider.TypeToHash;
            componentHashToType = typeProvider.HashToType;
            SetComponentsSetters();
            SetSystemSetters();
        }

        static partial void SetComponentsSetters();
        static partial void SetSystemSetters();

        public static void BindSystem<T>(in T system) where T: ISystem
        {
            var key = system.GetType();
            systemsSetters[key].BindSystem(system);
        }

        public static void UnBindSystem<T>(T system) where T : ISystem
        {
            var key = system.GetType();
            systemsSetters[key].UnBindSystem(system);
        }

        public static void SetComponent(IEntity entity, IComponent component)
        {
            componentsSetters[component.ComponentsMask.Index].SetComponent(entity, component);
        }

        public static void RemoveComponent(IEntity entity, IComponent component)
        {
            componentsSetters[component.ComponentsMask.Index].RemoveComponent(entity, component);
        }

        public static void RegisterComponent(int index, IEntity entity, bool isAdded)
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

        /// <summary>
        /// Static extention for getting by generic type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Owner"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetHECSComponent<T>(this IEntity Owner)
        {
            var t = typeof(T);

            if (TypeToComponentIndex.TryGetValue(t, out var index))
                return (T)Owner.GetAllComponents[index];
            else
            {
                var components = Owner.GetAllComponents;
                var count = components.Length;
                
                for (int i = 0; i < count; i++)
                {
                    if (components[i] is T needed)
                        return needed;
                }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetHECSComponent<T>(this IEntity Owner, ref HECSMask hECSMask)
        {
            return (T)Owner.GetAllComponents[hECSMask.Index];
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
        void SetComponent(IEntity entity, IComponent component);
        void RemoveComponent(IEntity entity, IComponent component);
        void RegisterComponent(IEntity entity, bool isAdded);
    }

    public interface ISystemSetter 
    {
        void BindSystem(ISystem system);
        void UnBindSystem(ISystem system);
    }
}