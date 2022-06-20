using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Abilities, Doc.HECS, "This system execute all abilities on ability holder, its usable if we have composite ability concept - ability what use damage and freeze effect same time, then we need create ability for each effect and run them through this system")]
    public sealed partial class CompositeAbilitiesSystem : BaseAbilityNoPredicatesSystem, ICompositeAbilitiesSystem, IReactCommand<ExecutePassiveAbilityCommand>
    {
        private HECSMask passiveAbilMask = HMasks.GetMask<PassiveAbilityTag>();
        public AbilitiesHolderComponent abilitiesHolder;
        private HECSMask abilityOwner = HMasks.GetMask<AbilityOwnerComponent>();

        public override void InitSystem()
        {
            abilitiesHolder = Owner.GetHECSComponent<AbilitiesHolderComponent>();
        }

        public override void Execute(IEntity owner = null, IEntity target = null, bool enable = true)
        {
            foreach (var a in abilitiesHolder.Abilities)
            {
                if (a.ContainsMask(ref passiveAbilMask)) continue;

                if (Owner.TryGetHecsComponent(abilityOwner, out AbilityOwnerComponent abilityOwnerComponent))
                    a.GetOrAddComponent<AbilityOwnerComponent>(abilityOwner).AbilityOwner = abilityOwnerComponent.AbilityOwner;
                else
                    a.GetOrAddComponent<AbilityOwnerComponent>(abilityOwner).AbilityOwner = Owner;

                a.Command(new ExecuteAbilityCommand
                {
                    Owner = owner,
                    Target = target,
                    Enabled = enable
                });
            }
        }

        public void CommandReact(ExecutePassiveAbilityCommand command)
        {
                foreach (var a in abilitiesHolder.Abilities)
                {
                    if (!a.ContainsMask(ref passiveAbilMask)) continue;

                    a.GetOrAddComponent<AbilityOwnerComponent>(abilityOwner).AbilityOwner = Owner.GetHECSComponent<AbilityOwnerComponent>(ref abilityOwner).AbilityOwner;

                    a.Command(new ExecuteAbilityCommand
                    {
                        Owner = command.Owner,
                        Enabled = command.Enabled,
                    });
                }
        }
    }

    public interface ICompositeAbilitiesSystem : ISystem { }
}