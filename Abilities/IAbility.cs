namespace HECSFramework.Core
{
    public interface IAbility : IEntity
    {
        void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}