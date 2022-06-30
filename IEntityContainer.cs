namespace HECSFramework.Core
{
    public interface IEntityContainer
    {
        public void Init(IEntity entity);
        public string ContainerID { get; }
    }
}