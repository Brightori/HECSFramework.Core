using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Feature("Additional Ability")]
    [Documentation(Doc.Abilities, Doc.Tag, Doc.HECS, "This is an ability tag, we add it to the ability container")]
    public  class AbilityTagComponent : BaseComponent
    {
    }
}