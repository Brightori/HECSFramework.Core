namespace HECSFramework.Core
{
    public static partial class StandartEntitiesExtentions
    {
        public static void SetWorld(this IEntity entity, World world)
        {
            world.MigrateEntityToWorld(entity);
        }

        public static bool IsAlive(this IEntity entity)
        {
            return entity != null && entity.IsAlive;
        }
    }
}