namespace HECSFramework.Core
{
    public struct ExecuteReward : ICommand
    {
        public Entity Owner;
        public Entity Target;
    }
}