namespace HECSFramework.Core
{
    public interface IBeforeMigrationToWorld
    {
        void BeforeMigration();
    }

    public interface IAfterMigrationToWorld
    {
        void AfterMigration();
    }
}