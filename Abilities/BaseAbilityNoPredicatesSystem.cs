using Commands;
using Components;

namespace HECSFramework.Core
{
    public abstract class BaseAbilityNoPredicatesSystem : BaseSystem, IAbilitySystem, IReactCommand<ExecuteAbilityCommand>
    {
        private HECSMask actorContainerIDMask = HMasks.GetMask<ActorContainerID>();

        public void CommandReact(ExecuteAbilityCommand command)
        {
            Execute(command.Owner, command.Target, command.Enabled);
            
            Owner.Command(new AbilityWasExecutedCommand
            {
                AbilityIndex = Owner.GetHECSComponent<ActorContainerID>(ref actorContainerIDMask).ContainerIndex
            });
        }

        public abstract void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}