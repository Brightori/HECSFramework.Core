namespace HECSFramework.Core
{
    public interface IPredicate : IHaveOwner
    {
        bool IsReady(IEntity target);
        void Init(IEntity owner, IAbility ability);
    }

    public abstract class BasePredicate : IPredicate
    {
        public IEntity Owner { get; set; }
        protected IAbility ability { get; set; }

        public void Init(IEntity owner, IAbility ability)
        {
            Owner = owner;
            this.ability = ability;
            LocalInit(owner);
        }

        public abstract void LocalInit(IEntity owner);

        public abstract bool IsReady(IEntity target);
    }

    public interface IPredicateContainer
    {
        IPredicate GetPredicate { get; }
    }
}