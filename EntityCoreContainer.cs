using Components;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public abstract class EntityCoreContainer
    {
        public abstract string ContainerID { get; }
        public List<IComponent> Components = new List<IComponent>();
        public List<ISystem> Systems = new List<ISystem>();

        public EntityCoreContainer()
        {
            Components = GetComponents();
            Systems = GetSystems();
        }

        protected abstract List<IComponent> GetComponents();
        protected abstract List<ISystem> GetSystems();

        public virtual void Init(IEntity entityForInit, bool pure = false)
        {
            var entity = new Entity(ContainerID);
            entity.AddHecsComponent(new ActorContainerID { ID = ContainerID });
            foreach (var component in Components)
            {
                if (component == null)
                    continue;

                entity.AddHecsComponent(component, entity);
            }

            foreach (var system in Systems)
            {
                if (system == null)
                    continue;

                entity.AddHecsSystem(system, entity);
            }

            var resolver = new EntityResolver().GetEntityResolver(entity);
            entityForInit.LoadEntityFromResolver(resolver);
        }
    }
}