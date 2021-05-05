
using Commands;

namespace HECSFramework.Core
{
    public interface AbilitySystem : ISystem
    {
        void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }

    public interface IActiveAbilitySystem : AbilitySystem, IReactCommand<ExecuteActiveAbilityCommand> {}  

    public interface IPassiveAbilitySystem : AbilitySystem { }
}