using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.Abilities, "we send this command when what start scenarion of adding command")]
    public struct AddAbilityCommand : ICommand
    {
        public Entity Entity;
    }
}
