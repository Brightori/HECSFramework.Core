using Commands;
using Components;

namespace HECSFramework.Core
{
    [Documentation(Doc.Abilities, "Basic system for active abilities, which are launched every time on demand, through receiving a command")]
    public abstract class BaseAbilitySystem : BaseSystem, IActiveAbilitySystem
    {
        public void CommandReact(ExecuteAbilityCommand command)
        {
            if (command.Enabled && !command.IgnorePredicates && Owner.TryGetComponent(out AbilityPredicateComponent predicatesComponent))
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

        public abstract void Execute(Entity owner = null, Entity target = null, bool enable = true);
    }
}