using Commands;

namespace HECSFramework.Core
{
    public interface IAbilitySystem : ISystem, IAbility
    {
        void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}