using System.Collections.Generic;

namespace HECSFramework.Core
{
    public interface IAbility : IHavePredicates, IHaveOwner, IHaveComponents, ICanAddComponents, IHaveSystems
    {
        void Execute<T>(T owner, T target = default, bool enable = true) where T : IEntity;
        void SetupAbilityData(List<IComponent> components, List<ISystem> systems, List<IPredicate> predicates);
    }

    public interface IHaveComponents
    {
        bool TryGetHecsComponent<T>(out T component) where T : IComponent;
    }

    public interface ICanAddComponents
    {
        void AddHecsComponent(IComponent component);
    }

    public interface IHaveSystems
    {
        bool TryGetHecsSystem<T>(out T system) where T : ISystem;
    }

    public interface IHavePredicates
    {
        bool IsReady(IEntity target);
    }
}