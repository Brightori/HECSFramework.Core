using Commands;
using Components;

namespace HECSFramework.Core
{
    [Documentation(Doc.Abilities, "Basic system for passive abilities, they are launched through the command only at the stage of adding, 1 time")]
    public abstract class BasePassiveAbilitySystem : BaseSystem, IPassiveAbilitySystem
    {
        private HECSMask predicateMask = HMasks.GetMask<AbilityPredicateComponent>();

        public void CommandReact(ExecutePassiveAbilityCommand command)
        {
            if (command.Enabled && Owner.TryGetHecsComponent(predicateMask, out AbilityPredicateComponent predicatesComponent))
            {
                if (!predicatesComponent.TargetPredicates.IsReady(command.Target, Owner))
                    return;

                if (!predicatesComponent.AbilityPredicates.IsReady(Owner))
                    return;

                if (!predicatesComponent.AbilityOwnerPredicates.IsReady(command.Owner, command.Target))
                    return;
            }

            Execute(command.Owner, command.Target, command.Enabled);

            Owner.Command(new AbilityWasExecutedCommand { Enabled = command.Enabled });
        }

        public abstract void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}