using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Feature("BaseAbilities")]
    [Documentation(Doc.Abilities, Doc.HECS, "Main system for operating abilities")]
    public sealed partial class AbilitiesSystem : BaseSystem, IAfterEntityInit, IReactCommand<ExecuteAbilityByIDCommand>, IReactCommand<AddAbilityCommand>
    {
        [Required]
        public AbilitiesHolderComponent abilitiesHolderComponent;

        public void AfterEntityInit()
        {
            Owner.Command(new AbilitiesReadyCommand());
        }

        public void CommandReact(ExecuteAbilityByIDCommand command)
        {
            if (abilitiesHolderComponent.IndexToAbility.TryGetValue(command.AbilityIndex, out var ability))
            {
                ability.Command(new ExecuteAbilityCommand { Enabled = command.Enable, IgnorePredicates = command.IgnorePredicates, Owner = command.Owner, Target = command.Target });
            }
            else
            {
                HECSDebug.LogWarning($"{Owner.ID} doesnt have ability with index {command.AbilityIndex}");
            }
        }

        public void CommandReact(AddAbilityCommand command)
        {
            abilitiesHolderComponent.AddAbility(command.Entity);
            ProcessViewReady(command.Entity);
        }

        public override void InitSystem()
        {
            ClientInit();
        }

        partial void ProcessViewReady(Entity entity);

        partial void ClientInit();
    }
}