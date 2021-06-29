namespace HECSFramework.Core
{
    public interface IPredicate
    {
        bool IsReady(IEntity target);
    }

    public interface IPredicateContainer
    {
        IPredicate GetPredicate { get; }
    }
}