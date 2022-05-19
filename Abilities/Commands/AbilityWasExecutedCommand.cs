using HECSFramework.Core;

namespace Commands
{
    public struct AbilityWasExecutedCommand : ICommand
    {
        public IAbility Ability;
        public bool Enabled;
    }
}