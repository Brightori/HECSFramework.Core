namespace HECSFramework.Core
{
    public interface IComponent : IHaveOwner
    {
        int GetTypeHashCode { get; }
        HECSMask ComponentsMask { get; set; }
        bool IsAlive { get; set; }
    }

    [Documentation(Doc.HECS, "Interface tag for singleton components at the world")]
    public interface IWorldSingleComponent : IComponent { }
}