using HECSFramework.Core.Helpers;
using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class EntityFilter : IDisposable
    {
        private Dictionary<int, Filter> filters = new Dictionary<int, Filter>(16);
        private World world;

        public EntityFilter(World world)
        {
            this.world = world;
        }

        public void Dispose()
        {
            foreach (var f in filters)
                f.Value.Dispose();
        }

        public List<IEntity> GetFilter(HECSMask include)
        {
            return GetFilter(include, HECSMask.Empty);
        }

        public List<IEntity> GetFilter(HECSMask include, HECSMask exclude)
        {
            int sumMask = include.GetHashCode();
            sumMask += exclude.GetHashCode();

            if (filters.TryGetValue(sumMask, out var filter))
                return filter.Entities;

            var nf = new Filter(world, include, exclude);

            filters.Add(sumMask, nf);
            return nf.Entities;
        }

        private class Filter : IReactComponent, IDisposable, IReactEntity
        {
            private List<IEntity> entities = new List<IEntity>(32);
            private readonly World world;
            private HECSMask mask;
            private HECSMask excludeMask;

            public List<IEntity> Entities => entities;

            public System.Guid ListenerGuid { get; } = Guid.NewGuid();

            public Filter(World world, HECSMask includeComponents, HECSMask excludeComponents)
            {
                this.world = world;
                mask = includeComponents;
                excludeMask = excludeComponents;
                world.AddGlobalReactComponent(this);
                world.AddEntityListener(this, true);
                GatherEntities(world);
            }

            private void GatherEntities(World world)
            {
                lock (world.Entities)
                {
                    var worldEntities = world.Entities;
                    var count = world.EntitiesCount;

                    for (int i = 0; i < count; i++)
                    {
                        if (!worldEntities[i].IsInited)
                            continue;

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

            public void Dispose()
            {
                world.RemoveGlobalReactComponent(this);
                world.AddEntityListener(this, false);
            }

            public void EntityReact(IEntity entity, bool isAdded)
            {
                if (isAdded && entity.ContainsMask(ref mask) && !entity.ContainsMask(ref excludeMask))
                {
                    entities.AddOrRemoveElement(entity, true);
                    return;
                }
                    
                if (!isAdded && entity.ContainsMask(ref mask))
                    entities.AddOrRemoveElement(entity, false);
            }
        }
    }
}