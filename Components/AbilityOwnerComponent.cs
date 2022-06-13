using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Abilities, Doc.HECS, "This component is required for the ability, the character that owns it is thrown into the owner")]
    public class AbilityOwnerComponent : BaseComponent
    {
        public IEntity AbilityOwner;
    }
}