using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.Abilities, "active abilities are those that are triggered every time on demand, or have an effect over time")]
    public partial struct ExecuteAbilityCommand : ICommand
    {
        public IEntity Target;
        public IEntity Owner;
        public bool Enabled;
        public bool IgnorePredicates;
    }
}