namespace HECSFramework.Core
{
    public struct ExecuteReward : ICommand
    {
        public IEntity Owner;
        public IEntity Target;
    }
}