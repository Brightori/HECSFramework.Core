using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public static partial class StandartEntitiesExtentions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponent<T>(this Entity entity) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].GetComponent(entity.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IComponent GetComponent(this Entity entity, int typeIndex)
        {
            return entity.World.GetComponentProvider(typeIndex).GetIComponent(entity.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrAddComponent<T>(this Entity entity) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].GetOrAddComponent(entity.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrAddComponent<T>(this Entity entity, T component) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].GetOrAddComponent(entity.Index, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddComponent<T>(this Entity entity) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].AddComponent(entity.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddComponent<T>(this Entity entity, T component) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].AddComponent(entity.Index, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IComponent AddComponent(this Entity entity, IComponent component) 
        {
            var typeIndex = TypesMap.GetComponentInfo(component);
            var provider = entity.World.GetComponentProvider(typeIndex.ComponentsMask.TypeHashCode);
            provider.AddComponent(entity.Index, component);
            return component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetComponent<T>(this Entity entity, out T component) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].TryGetComponent(entity.Index, out component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent<T>(this Entity entity) where T : IComponent, new()
        {
            ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].RemoveComponent(entity.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent(this Entity entity, IComponent component) 
        {
            entity.World.GetComponentProvider(component.GetTypeHashCode).RemoveComponent(entity.Index);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent(this Entity entity, int typeIndex)
        {
            entity.World.GetComponentProvider(typeIndex).RemoveComponent(entity.Index);
        }
    }
}
