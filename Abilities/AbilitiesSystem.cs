using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Documentation(Doc.Abilities, Doc.HECS, "Main system for operating abilities")]
    public sealed partial class AbilitiesSystem : BaseSystem, IAfterEntityInit, IReactCommand<ExecuteAbilityByIDCommand>
    {
        [Required]
        public AbilitiesHolderComponent abilitiesHolderComponent;

        public void AfterEntityInit()
        {
            foreach (var a in abilitiesHolderComponent.Abilities)
                a.Init();

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

        public override void InitSystem()
        {
            ClientInit();
        }

        partial void ClientInit();
    }
}