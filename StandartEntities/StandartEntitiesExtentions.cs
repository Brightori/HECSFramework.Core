using System.Runtime.CompilerServices;

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
            return entity != null && entity.IsAlive;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this Entity entity, int generation)
        {
            return entity != null && entity.IsAlive && generation == entity.Generation;
        }
    }
}