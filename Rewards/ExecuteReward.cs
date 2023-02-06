using HECSFramework.Core;

namespace Commands
{
    public struct ExecuteReward : ICommand
    {
        public Entity Owner;
        public Entity Target;
    }
}