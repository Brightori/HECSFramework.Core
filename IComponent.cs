namespace HECSFramework.Core
{
    public interface IComponent  : IHaveOwner, IBeforeEntityDispose
    {
        bool IsAlive { get; set; }
        int GetTypeHashCode { get; }
    }

    [Documentation(Doc.HECS, "Interface tag for singleton components at the world")]
    public interface IWorldSingleComponent : IComponent { }

    [Documentation(Doc.HECS, "Interface tag for components what we need to repool")]
    public interface IPoolableComponent : IComponent { }
}