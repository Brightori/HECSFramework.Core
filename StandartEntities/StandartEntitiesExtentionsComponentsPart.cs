using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public static partial class StandartEntitiesExtentions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponent<T>(this IEntity entity) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].GetComponent(entity.EntityIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrAddComponent<T>(this IEntity entity) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].GetOrAddComponent(entity.EntityIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrAddComponent<T>(this IEntity entity, T component) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].GetOrAddComponent(entity.EntityIndex, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddComponent<T>(this IEntity entity) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].AddComponent(entity.EntityIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddComponent<T>(this IEntity entity, T component) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].AddComponent(entity.EntityIndex, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IComponent AddComponent(this IEntity entity, IComponent component) 
        {
            var typeIndex = TypesMap.GetComponentInfo(component);
            var provider = entity.World.GetComponentProvider(typeIndex.ComponentsMask.Index);
            provider.AddComponent(entity.EntityIndex, component);
            return component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetComponent<T>(this IEntity entity, out T component) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].TryGetComponent(entity.EntityIndex, out component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent<T>(this IEntity entity) where T : IComponent, new()
        {
            ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].RemoveComponent(entity.EntityIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent(this IEntity entity, IComponent component) 
        {
            entity.World.GetComponentProvider(component.GetTypeHashCode).RemoveComponent(entity.EntityIndex);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent(this IEntity entity, int typeIndex)
        {
            entity.World.GetComponentProvider(typeIndex).RemoveComponent(entity.EntityIndex);
        }
    }
}
