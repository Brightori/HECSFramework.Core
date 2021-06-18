using HECSFramework.Core.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

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
            
            private HECSMask mask;
            private HECSMask excludeMask;
            
            private HashSet<Guid> entitiesAtFilter = new HashSet<Guid>();

            public ConcurrencyList<IEntity> Entities { get; private set; } = new ConcurrencyList<IEntity>();

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
                var worldEntities = world.Entities;
                var count = world.EntitiesCount;

                for (int i = 0; i < count; i++)
                {
                    var currentEntity = worldEntities[i];

                    if ( currentEntity == null || !currentEntity.IsInited || !currentEntity.IsAlive)
                        continue;

                    if (currentEntity.ContainsMask(ref mask) && !currentEntity.ContainsMask(ref excludeMask))
                    {
                        Entities.Add(currentEntity);
                        entitiesAtFilter.Add(currentEntity.GUID);
                    }
                }
            }

            public void ComponentReact(IComponent component, bool isAdded)
            {
                if (entitiesAtFilter.Contains(component.Owner.GUID))
                {
                    if (isAdded)
                        return;
                    else
                    {
                        entitiesAtFilter.Remove(component.Owner.GUID);
                        Entities.Remove(component.Owner);
                    }
                }

                if (!component.Owner.ContainsMask(ref mask))
                    return;

                if (component.Owner.ContainsMask(ref excludeMask))
                    return;

                Entities.AddOrRemoveElement(component.Owner, isAdded);
                entitiesAtFilter.AddOrRemoveElement(component.Owner.GUID, isAdded);
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
                    Entities.AddOrRemoveElement(entity, true);
                    entitiesAtFilter.Add(entity.GUID);
                    return;
                }

                if (!isAdded && entity.ContainsMask(ref mask))
                {
                    Entities.AddOrRemoveElement(entity, false);
                    entitiesAtFilter.AddOrRemoveElement(entity.GUID, false);
                }
            }
        }
    }
}