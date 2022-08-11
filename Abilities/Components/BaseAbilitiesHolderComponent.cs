using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Abilities, "This component holds current|default abilities, we operate this throw abilitis system")]
    public sealed partial class AbilitiesHolderComponent : BaseComponent, IInitable
    {
        [HideInInspectorCrossPlatform]
        private List<IEntity> abilities = new List<IEntity>(8);
        public ReadonlyList<IEntity> Abilities;

        public void Init()
        {
            Abilities = new ReadonlyList<IEntity>(abilities);
        }

        public void AddAbility (IEntity ability, bool needInit = false)
        {
            abilities.Add(ability);

            if (needInit)
                ability.Init();
        }

        public void RemoveAbility(IEntity ability)
        {
            abilities.Remove(ability);
        }
    }
}