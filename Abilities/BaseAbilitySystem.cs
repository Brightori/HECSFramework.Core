using Commands;
using Components;

namespace HECSFramework.Core
{
    [Documentation(Doc.Abilities, "Basic system for active abilities, which are launched every time on demand, through receiving a command")]
    public abstract class BaseAbilitySystem : BaseSystem, IActiveAbilitySystem
    {
        private HECSMask predicateMask = HMasks.GetMask<AbilityPredicateComponent>();

        public void CommandReact(ExecuteAbilityCommand command)
        {
            if (command.Enabled && !command.IgnorePredicates && Owner.TryGetHecsComponent(predicateMask, out AbilityPredicateComponent predicatesComponent))
            {
                if (!predicatesComponent.TargetPredicates.IsReady(command.Target, Owner))
                    return;

                if (!predicatesComponent.AbilityPredicates.IsReady(Owner))
                    return;

                if (!predicatesComponent.AbilityOwnerPredicates.IsReady(command.Owner, command.Target))
                    return;
            }

            Execute(command.Owner, command.Target, command.Enabled);
            Owner.Command(new AbilityWasExecutedCommand { Ability = this,  Enabled = command.Enabled });
        }

        public abstract void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}