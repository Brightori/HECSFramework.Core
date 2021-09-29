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

        public ConcurrencyList<IEntity> GetFilter(FilterMask include)
        {
            int sumMask = include.GetHashCode();

            if (filters.TryGetValue(sumMask, out var filter))
                return filter.Entities;

            var nf = new Filter(world, include, new FilterMask());

            filters.Add(sumMask, nf);
            return nf.Entities;
        }

        public ConcurrencyList<IEntity> GetFilter(FilterMask include, FilterMask exclude)
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
            private readonly World world;

            private HashSet<Guid> entitiesAtFilter = new HashSet<Guid>();

            private HECSMultiMask summaryInclude;
            private HECSMultiMask summaryExclude;

            public ConcurrencyList<IEntity> Entities { get; private set; } = new ConcurrencyList<IEntity>();

            public System.Guid ListenerGuid { get; } = Guid.NewGuid();

            private Queue<IEntity> removeQueue = new Queue<IEntity>();

            public Filter(World world, FilterMask include, FilterMask exclude)
            {
                this.world = world;

                summaryInclude = new HECSMultiMask(include);
                summaryExclude = new HECSMultiMask(exclude);

                world.AddGlobalReactComponent(this);
                world.AddEntityListener(this, true);
                GatherEntities(world);
            }

            private void GatherEntities(World world)
            {
                var worldEntities = world.Entities;
                var count = world.EntitiesCount;

                for (int i = 0; i < count; i++)
                {
                    var currentEntity = worldEntities[i];

                    if (currentEntity == null || !currentEntity.IsInited || !currentEntity.IsAlive)
                        continue;

                    if (ContainsMask(currentEntity))
                    {
                        Entities.Add(currentEntity);
                        entitiesAtFilter.Add(currentEntity.GUID);
                    }
                }
            }

            private bool ContainsMask(IEntity target)
            {
                return target.ContainsMask(summaryInclude) && !target.ContainsMask(summaryExclude);
            }


            public void ComponentReact(IComponent component, bool isAdded)
            {
                if (summaryInclude.Contains(component.ComponentsMask) || summaryExclude.Contains(component.ComponentsMask))
                {
                    if (isAdded)
                    {
                        var entity = component.Owner;

                        if (ContainsMask(entity))
                        {
                            Entities.AddOrRemoveElement(entity, true);
                            entitiesAtFilter.Add(entity.GUID);
                        }
                    }
                    else
                    {
                        if (entitiesAtFilter.Contains(component.Owner.GUID))
                        {
                            if (ContainsMask(component.Owner))
                                return;

                            entitiesAtFilter.Remove(component.Owner.GUID);
                            Entities.Remove(component.Owner);
                        }
                        else
                        {
                            if (ContainsMask(component.Owner))
                            {
                                entitiesAtFilter.Add(component.Owner.GUID);
                                Entities.Add(component.Owner);
                            }
                        }
                    }
                }
            }

            public void Dispose()
            {
                world.RemoveGlobalReactComponent(this);
                world.AddEntityListener(this, false);
            }

            public void EntityReact(IEntity entity, bool isAdded)
            {
                if (isAdded && ContainsMask(entity))
                {
                    Entities.AddOrRemoveElement(entity, true);
                    entitiesAtFilter.Add(entity.GUID);
                    return;
                }

                if (!isAdded && entitiesAtFilter.Contains(entity.GUID))
                {
                    Entities.AddOrRemoveElement(entity, false);
                    entitiesAtFilter.AddOrRemoveElement(entity.GUID, false);
                }
            }
        }
    }
}