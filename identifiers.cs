namespace HECSFramework.Core
{
    public interface IIdentifier
    {
        int Id { get; }
    }

    public class Identifier : IIdentifier
    {
        public int Id { get; set; }
    }
}