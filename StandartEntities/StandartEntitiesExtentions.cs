using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        public static T AddComponent<T>(this IEntity entity) where T: IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].AddComponent(entity.EntityIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddComponent<T>(this IEntity entity, T component) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].AddComponent(entity.EntityIndex, component);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrAddComponent<T>(this IEntity entity) where T : IComponent, new()
        {
            return  ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].GetOrAddComponent(entity.EntityIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrAddComponent<T>(this IEntity entity, T component) where T : IComponent, new()
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[entity.World.Index].GetOrAddComponent(entity.EntityIndex, component);
        }
    }
}
