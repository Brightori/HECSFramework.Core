using Commands;
using Components;

namespace HECSFramework.Core
{
    public abstract class BaseAbilityNoPredicatesSystem : BaseSystem, IAbilitySystem, IReactCommand<ExecuteAbilityCommand>
    {
        private HECSMask predicateMask = HMasks.GetMask<AbilityPredicateComponent>();

        public void CommandReact(ExecuteAbilityCommand command)
        {
            Execute(command.Owner, command.Target, command.Enabled);
            Owner.Command(new AbilityWasExecutedCommand { AbilityIndex = Owner.GetActorContainerID().ContainerIndex });
        }

        public abstract void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}