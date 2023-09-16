using System;
using System.Collections.Generic;
using HECSFramework.Core;
using HECSFramework.Core.Helpers;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Abilities, "This component holds current|default abilities, we operate this throw abilitis system")]
    public sealed partial class AbilitiesHolderComponent : BaseComponent, IInitable, IDisposable
    {
        [HideInInspectorCrossPlatform]
        public List<Entity> Abilities = new List<Entity>(8);
        public Dictionary<int, Entity> IndexToAbility = new Dictionary<int, Entity>();

        public void Init()
        {
        }

        public void AddAbility(Entity ability, bool needInit = false)
        {
            ability.GetOrAddComponent<AbilityOwnerComponent>().AbilityOwner = Owner;
            Abilities.Add(ability);
            IndexToAbility.Add(ability.GetComponent<ActorContainerID>().ContainerIndex, ability);

            if (ability.ContainsMask<InitOnAddAbilityTagComponent>())
                ability.Init();

            if (ability.TryGetComponent(out AdditionalAbilityIndexComponent component))
            {
                foreach (var i in component.AdditionalIndeces)
                    IndexToAbility.AddOrReplace(i, ability);
            }

            if (needInit)
                ability.Init();
        }

        public void RemoveAbility(Entity ability)
        {
            Abilities.Remove(ability);
            IndexToAbility.Remove(ability.GetComponent<ActorContainerID>().ContainerIndex);

            if (ability.TryGetComponent(out AdditionalAbilityIndexComponent component))
            {
                foreach (var i in component.AdditionalIndeces)
                    IndexToAbility.Remove(i);
            }

            ability.Dispose();
        }

        public void Dispose()
        {
            foreach (var a in Abilities)
                a.Dispose();

            Abilities.Clear();
            IndexToAbility.Clear();
        }
    }
}