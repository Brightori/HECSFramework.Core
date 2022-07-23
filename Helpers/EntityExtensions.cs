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
        /// this methods return structure, this us needed for case when we add new component, add him after setup data,
        /// we need call process data after 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GetAndSetComponent<T> GetAndSetComponent<T>(this IEntity entity, HECSMask mask) where T : class, IComponent
        {
            if (entity.TryGetHecsComponent(mask, out T component))
                return new GetAndSetComponent<T>
                {
                    Component = component,
                    Entity = entity,
                    IsAlrdyOnEntity = true,
                };
            else
            {
                var comp = entity.World.GetSingleSystem<PoolingSystem>().GetComponentFromPool<T>(ref mask);
                return new GetAndSetComponent<T>
                {
                    Component = comp,
                    Entity = entity,
                    IsAlrdyOnEntity = false,
                };
            }
        }

        /// <summary>
        /// Complete cheking of entity, include checking for null and alive entity manager
        /// </summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this IEntity entity)
            => EntityManager.IsAlive && entity != null && entity.IsAlive;
    }

    [Documentation(Doc.HECS, "we need this structure when we add or get component, but want put here info, and after add this component to entity")]
    public struct GetAndSetComponent<T> where T : IComponent
    {
        public T Component;
        public IEntity Entity;
        public bool IsAlrdyOnEntity;

        public void ProccessAfterSetData()
        {
            if (IsAlrdyOnEntity)
                return;

            Entity.AddHecsComponent(Component);
        }
    }
}