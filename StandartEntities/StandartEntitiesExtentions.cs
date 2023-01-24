namespace HECSFramework.Core
{
    public static partial class StandartEntitiesExtentions
    {
        public static void SetWorld(this Entity entity, World world)
        {
            world.MigrateEntityToWorld(entity);
        }

        public static bool IsAlive(this Entity entity)
        {
            return entity != null && entity.IsAlive;
        }
    }
}