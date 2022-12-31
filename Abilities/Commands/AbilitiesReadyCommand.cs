using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Abilities, "we send this command after init abilities, if we need put some logic after abilities we should listen this command ")]
    public struct AbilitiesReadyCommand : ICommand
    {
    }
}