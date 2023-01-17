using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Abilities, "This component holds current|default abilities, we operate this throw abilitis system")]
    public sealed partial class AbilitiesHolderComponent : BaseComponent, IInitable, IDisposable
    {
        [HideInInspectorCrossPlatform]
        private List<IEntity> abilities = new List<IEntity>(8);
        public ReadonlyList<IEntity> Abilities;
        public Dictionary<int, IEntity> IndexToAbility = new Dictionary<int, IEntity>();

        public void Init()
        {
            Abilities = new ReadonlyList<IEntity>(abilities);
        }

        public void AddAbility(IEntity ability, bool needInit = false)
        {
            abilities.Add(ability);
            IndexToAbility.Add(ability.GetComponent<ActorContainerID>().ContainerIndex, ability);

            if (needInit)
                ability.Init(Owner.World);
        }

        public void RemoveAbility(IEntity ability)
        {
            abilities.Remove(ability);
            IndexToAbility.Remove(ability.GetComponent<ActorContainerID>().ContainerIndex);
        }

        public void CleanAbilities()
        {
            foreach (var a in abilities.ToArray())
            {
                a.Dispose();
            }

            abilities.Clear();
            IndexToAbility.Clear();
        }

        public void Dispose()
        {
            CleanAbilities();
            abilities = null;
            IndexToAbility = null;
        }
    }
}