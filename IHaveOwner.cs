namespace HECSFramework.Core
{
    public interface IHaveOwner
    {
        IEntity Owner { get; set; }
    }
}