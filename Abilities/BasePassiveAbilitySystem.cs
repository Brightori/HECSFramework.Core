using Commands;
using Components;

namespace HECSFramework.Core
{
    [Documentation(Doc.Abilities, "Basic system for passive abilities, they are launched through the command only at the stage of adding, 1 time")]
    public abstract class BasePassiveAbilitySystem : BaseSystem, IPassiveAbilitySystem
    {
        public void CommandReact(ExecutePassiveAbilityCommand command)
        {
            if (command.Enabled && Owner.TryGetComponent(out AbilityPredicateComponent predicatesComponent))
            {
                if (!predicatesComponent.TargetPredicates.IsReady(command.Target, Owner))
                    return;

                if (!predicatesComponent.AbilityPredicates.IsReady(Owner))
                    return;

                if (!predicatesComponent.AbilityOwnerPredicates.IsReady(command.Owner, command.Target))
                    return;
            }

            Execute(command.Owner, command.Target, command.Enabled);
        }

        public abstract void Execute(Entity owner = null, Entity target = null, bool enable = true);
    }
}