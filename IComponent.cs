namespace HECSFramework.Core 
{
    public interface IComponent : IHaveOwner
    {
        int GetTypeHashCode { get; }
        HECSMask ComponentsMask { get; set; }
    }
}