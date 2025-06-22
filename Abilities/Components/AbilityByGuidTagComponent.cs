using HECSFramework.Core;

namespace Components
{
    [Documentation(Doc.Abilities, Doc.HECS, "we should add this component to ability, if we want operate ability through guid not index, this is needed for cases when we have many abilities from one container (like two revolvers etc)")]
    public sealed class AbilityByGuidTagComponent : BaseComponent
    {
    }
}