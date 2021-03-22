using HECSFramework.Core.Helpers;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class EntityFilter : IReactComponent
    {
        private List<IEntity> entities = new List<IEntity>(32);
        
        private HECSMask mask;
        private HECSMask excludeMask;

        public System.Guid ListenerGuid { get; }

        public EntityFilter (World world, HECSMask includeComponents)
        {
            mask = includeComponents;
            excludeMask = HECSMask.Empty;

            GatherEntities(world);
        }

        public EntityFilter (World world, HECSMask includeComponents, HECSMask excludeComponents)
        {
            mask = includeComponents;
            excludeMask = excludeComponents;

            GatherEntities(world);
        }

        private void GatherEntities(World world)
        {
            lock (world.Entities)
            {
                var worldEntities = world.Entities;
                var count = worldEntities.Length;

                for (int i = 0; i < count; i++)
                {
                    if (worldEntities[i].ContainsMask(ref mask) && !worldEntities[i].ContainsMask(ref excludeMask))
                        entities.Add(worldEntities[i]);
                }
            }
        }

        public void ComponentReact(IComponent component, bool isAdded)
        {
            if (!component.Owner.ContainsMask(ref mask))
                return;

            if (component.Owner.ContainsMask(ref excludeMask))
                return;

            lock (entities)
            {
                entities.AddOrRemoveElement(component.Owner, isAdded);
            }
        }
    }
}