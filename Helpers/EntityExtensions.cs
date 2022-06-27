using System.Runtime.CompilerServices;
using Systems;

namespace HECSFramework.Core
{
    internal static class EntityExtensions
    {
        /// <summary>
        /// Это самый быстрый способ получить или добавить и получить компонент, 
        /// в переборах и циклах лучше использовать его чем GetOrAdd,
        /// также этот метод под капотом содержит пуллинг, следите за данными которые выставляете
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="mask"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrAddComponent<T>(this IEntity entity, HECSMask mask) where T : class, IComponent
        {
            if (entity.TryGetHecsComponent(mask, out T component))
                return component;
            else
            {
                var comp = entity.World.GetSingleSystem<PoolingSystem>().GetComponentFromPool<T>(ref mask);
                entity.AddHecsComponent(comp);
                return comp;
            }
        }

        /// <summary>
        /// Complete cheking of entity, include checking for null and alive entity manager
        /// </summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this IEntity entity)
            => EntityManager.IsAlive && entity != null && entity.IsAlive;

        /// <summary>
        /// Get Entity from core container
        /// </summary>
        /// <param name="entityCoreContainer">Core contaner</param>
        /// <param name="entityName">Just name for understanding what kind of entity is it, if u dont assign any value, we take name from container </param>
        /// <param name="worldIndex">u should provide index of world</param>
        /// <returns></returns>
        public static Entity GetEntityFromCoreContainer(this EntityCoreContainer entityCoreContainer, int worldIndex = 0, string entityName = default)
        {
            Entity entity = null;

            if (entityCoreContainer == null)
            {
                HECSDebug.LogError("container is null");
                return null;
            }

            if (string.IsNullOrEmpty(entityName))
                entity = new Entity(entityName, worldIndex);
            else
                entity = new Entity(entityCoreContainer.ContainerID, worldIndex);

            entityCoreContainer.Init(entity);
            return entity;
        }
    }
}