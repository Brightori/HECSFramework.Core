using HECSFramework.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public ConcurrencyList<IEntity> GetFilter(HECSMask include)
        {
            return GetFilter(include, HECSMask.Empty);
        }

        public ConcurrencyList<IEntity> GetFilter(HECSMultiMask include)
        {
            return GetFilter(include, HECSMask.Empty);
        }

        public ConcurrencyList<IEntity> GetFilter(HECSMultiMask include, HECSMask exclude)
        {
            int sumMask = include.GetHashCode();
            sumMask += exclude.GetHashCode();

            if (filters.TryGetValue(sumMask, out var filter))
                return filter.Entities;

            var nf = new Filter(world, include, exclude);

            filters.Add(sumMask, nf);
            return nf.Entities;
        }

        public ConcurrencyList<IEntity> GetFilter(HECSMultiMask include, HECSMultiMask exclude)
        {
            int sumMask = include.GetHashCode();
            sumMask += exclude.GetHashCode();

            if (filters.TryGetValue(sumMask, out var filter))
                return filter.Entities;

            var nf = new Filter(world, include, exclude);

            filters.Add(sumMask, nf);
            return nf.Entities;
        }

        public ConcurrencyList<IEntity> GetFilter(HECSMask include, HECSMultiMask exclude)
        {
            int sumMask = include.GetHashCode();
            sumMask += exclude.GetHashCode();

            if (filters.TryGetValue(sumMask, out var filter))
                return filter.Entities;

            var nf = new Filter(world, include, exclude);

            filters.Add(sumMask, nf);
            return nf.Entities;
        }


        public ConcurrencyList<IEntity> GetFilter(HECSMask include, HECSMask exclude)
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
            private HashSet<HECSMask> include = new HashSet<HECSMask>();
            private HashSet<HECSMask> exclude = new HashSet<HECSMask>();

            private HECSMask summaryInclude;
            private HECSMask summaryExclude;

            public ConcurrencyList<IEntity> Entities { get; private set; } = new ConcurrencyList<IEntity>();

            public System.Guid ListenerGuid { get; } = Guid.NewGuid();

            private Queue<IEntity> removeQueue = new Queue<IEntity>();

            public Filter(World world, HECSMask includeComponents, HECSMask excludeComponents)
            {
                this.world = world;

                include.Add(includeComponents);
                exclude.Add(excludeComponents);

                world.AddGlobalReactComponent(this);
                world.AddEntityListener(this, true);

                summaryInclude = includeComponents;
                summaryExclude = excludeComponents;
                GatherEntities(world);
            }

            public Filter(World world, HECSMultiMask includeComponents, HECSMask excludeComponents)
            {
                this.world = world;

                AddMask(ref include, includeComponents);
                exclude.Add(excludeComponents);

                summaryExclude = GetMaskFromMultiMask(includeComponents);
                summaryInclude = excludeComponents;

                world.AddGlobalReactComponent(this);
                world.AddEntityListener(this, true);

                GatherEntities(world);
            }

            private HECSMask GetMaskFromMultiMask(HECSMultiMask multiMask)
            {
                return multiMask.A + multiMask.B + multiMask.C + multiMask.D;
            }

            public Filter(World world, HECSMask includeComponents, HECSMultiMask excludeComponents)
            {
                this.world = world;

                include.Add(includeComponents);
                AddMask(ref exclude, excludeComponents);

                summaryInclude = includeComponents;
                summaryExclude = GetMaskFromMultiMask(excludeComponents);

                world.AddGlobalReactComponent(this);
                world.AddEntityListener(this, true);

                GatherEntities(world);
            }

            public Filter(World world, HECSMultiMask includeComponents, HECSMultiMask excludeComponents)
            {
                this.world = world;

                AddMask(ref include, includeComponents);
                AddMask(ref exclude, excludeComponents);

                summaryInclude = GetMaskFromMultiMask(includeComponents);
                summaryExclude = GetMaskFromMultiMask(excludeComponents);

                world.AddGlobalReactComponent(this);
                world.AddEntityListener(this, true);

                GatherEntities(world);
            }

            private void AddMask(ref HashSet<HECSMask> masks, HECSMultiMask processmask)
            {
                if (processmask.A.Index != HECSMask.Empty.Index)
                    masks.Add(processmask.A);

                if (processmask.B.Index != HECSMask.Empty.Index)
                    masks.Add(processmask.B);

                if (processmask.C.Index != HECSMask.Empty.Index)
                    masks.Add(processmask.C);

                if (processmask.D.Index != HECSMask.Empty.Index)
                    masks.Add(processmask.D);
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
                foreach (var incl in include)
                {
                    var mask = incl;

                    if (!target.ContainsMask(ref mask))
                        return false;
                }

                foreach (var excl in exclude)
                {
                    var mask = excl;
                    if (target.ContainsMask(ref mask))
                        return false;
                }

                return true;
            }

            public void ComponentReact(IComponent component, bool isAdded)
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
                    foreach (var e in Entities)
                    {
                        if (e == null || ContainsMask(e))
                            continue;

                        removeQueue.Enqueue(e);
                    }

                    while (removeQueue.Count > 0)
                    {
                        Entities.Remove(removeQueue.Dequeue());
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