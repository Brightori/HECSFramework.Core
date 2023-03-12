using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Abilities, "this component holds additional tags for ability, its can be used if we always have MainAbility, but can change ability behind this index")]
    public sealed partial class AdditionalAbilityIndexComponent : BaseComponent
    {
        [NonSerialized]
        public List<int> AdditionalIndeces = new List<int>();
    }
}
