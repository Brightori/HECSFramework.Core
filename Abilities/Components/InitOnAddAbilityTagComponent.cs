using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Abilities, Doc.Tag, "we add this tag on ability when we need init this ability before add their to ability holder")]
    public sealed class InitOnAddAbilityTagComponent : BaseComponent
    {
       
    }
}