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
        private List<Entity> abilities = new List<Entity>(8);
        public ReadonlyList<Entity> Abilities;
        public Dictionary<int, Entity> IndexToAbility = new Dictionary<int, Entity>();

        public void Init()
        {
            Abilities = new ReadonlyList<Entity>(abilities);
        }

        public void AddAbility(Entity ability, bool needInit = false)
        {
            abilities.Add(ability);
            IndexToAbility.Add(ability.GetComponent<ActorContainerID>().ContainerIndex, ability);

            if (needInit)
                ability.Init(Owner.World);
        }

        public void RemoveAbility(Entity ability)
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