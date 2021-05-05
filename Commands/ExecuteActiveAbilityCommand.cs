using HECSFramework.Core;

namespace Commands
{
    public struct ExecuteActiveAbilityCommand : ICommand
    {
        public IEntity Target;
        public IEntity Owner;
        public bool Enabled;
    }
}