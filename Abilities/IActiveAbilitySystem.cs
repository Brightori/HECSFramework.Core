using Commands;

namespace HECSFramework.Core
{
    public interface IActiveAbilitySystem : ISystem, IAbility, IReactCommand<ExecuteAbilityCommand>
    {
    }

    public interface IPassiveAbilitySystem : ISystem, IAbility, IReactCommand<ExecutePassiveAbilityCommand>
    {
    }
}