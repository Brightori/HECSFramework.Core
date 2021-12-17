using Commands;
using Components;

namespace HECSFramework.Core
{
    [Documentation(Doc.Ability, "Базовая система для абилок, по дефолту проверяет предикаты, и только потом исполняет абилку, если нужно постоянно проверять абилку и куладуны, то лучше добавить к базовой системе интерфейс " + nameof(IAbility) + " и реализовать там свою логику")]
    public abstract class BaseAbilitySystem : BaseSystem, IAbilitySystem
    {
        private HECSMask predicateMask = HMasks.GetMask<AbilityPredicateComponent>();

        public void CommandReact(ExecuteAbilityCommand command)
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

            if (command.Enabled)
                Owner.Command(new AbilityWasExecutedCommand());
        }

        public abstract void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}