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

        public override void InitSystem()
        {
            abilitiesHolder = Owner.GetComponent<AbilitiesHolderComponent>();
        }

        public override void Execute(Entity owner = null, Entity target = null, bool enable = true)
        {
            foreach (var a in abilitiesHolder.Abilities)
            {
                if (Owner.TryGetComponent(out AbilityOwnerComponent abilityOwnerComponent))
                    a.GetOrAddComponent<AbilityOwnerComponent>().AbilityOwner = abilityOwnerComponent.AbilityOwner;
                else
                    a.GetOrAddComponent<AbilityOwnerComponent>().AbilityOwner = Owner;

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
                    if (!a.ContainsMask<PassiveAbilityTag>()) continue;

                    a.GetOrAddComponent<AbilityOwnerComponent>().AbilityOwner = Owner.GetComponent<AbilityOwnerComponent>().AbilityOwner;

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