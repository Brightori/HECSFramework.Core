namespace HECSFramework.Core
{
    public partial interface IPredicate
    {
        bool IsReady(IEntity target);
    }

    public interface IPredicateContainer
    {
        IPredicate GetPredicate { get; }
    }
}