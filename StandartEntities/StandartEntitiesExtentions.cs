using System.Runtime.CompilerServices;
using Components;

namespace HECSFramework.Core
{
    public static partial class StandartEntitiesExtentions
    {
        public static void SetWorld(this Entity entity, World world)
        {
            world.MigrateEntityToWorld(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this Entity entity)
        {
            return entity != null && entity.IsAlive && !entity.IsDisposed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAliveAndNotDead(this Entity entity)
        {
            return entity != null && entity.IsAlive && !entity.IsDisposed && !entity.ContainsMask<IsDeadTagComponent>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this Entity entity, int generation)
        {
            return entity != null && entity.IsAlive && generation == entity.Generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this IComponent component)
        {
            return component != null && component.IsAlive && component.Owner.IsAlive();
        }
    }
}