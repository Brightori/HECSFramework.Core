using Commands;
using Components;

namespace HECSFramework.Core
{
    public abstract class BaseAbilityNoPredicatesSystem : BaseSystem, IActiveAbilitySystem, IReactCommand<ExecuteAbilityCommand>
    {
        public void CommandReact(ExecuteAbilityCommand command)
        {
            Execute(command.Owner, command.Target, command.Enabled);
            Owner.Command(new AbilityWasExecutedCommand { Ability = this, Enabled = command.Enabled });
        }

        public abstract void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}