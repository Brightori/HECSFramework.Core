namespace HECSFramework.Core
{
    public interface ITimeProvider : IComponent, IWorldSingleComponent
    {
        float DeltaTime { get; }
    }
}
