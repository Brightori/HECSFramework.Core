namespace HECSFramework.Core
{
    public interface IAbilitySystem : ISystem
    {
        void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}