using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HECSFramework.Core.Helpers;
using Helpers;

namespace HECSFramework.Core
{
    public partial class World
    {
        public IEntity[] Entities;
        private HECSList<IReactEntity> reactEntities = new HECSList<IReactEntity>();
        
        //here we have free entities for pooling|using
        private Queue<int> freeIndices = new Queue<int>();

        //dirty entities should be processing 
        private HECSList<int> dirtyEntities = new HECSList<int>(32);
        private HECSList<RegisterEntity> registerEntity = new HECSList<RegisterEntity>(32);

        private Dictionary<int, ComponentProvider> componentProvidersByTypeIndex = new Dictionary<int, ComponentProvider>(256);
        private Dictionary<int, EntitiesFilter> entitiesFilters = new Dictionary<int,EntitiesFilter>(8);

        public EntitiesFilter GetFilter<T>() where T : IComponent, new() => GetFilterFromCache(Filter.Get<T>(), new Filter());
        public EntitiesFilter GetFilter(Filter inclide) => GetFilterFromCache(inclide, new Filter());
        public EntitiesFilter GetFilter(Filter inclide, Filter exclude) => GetFilterFromCache(inclide, exclude);

        private EntitiesFilter GetFilterFromCache(Filter include, Filter exclude)
        {
            var key = include.GetHashCode() + exclude.GetHashCode();

            if (entitiesFilters.TryGetValue(key, out var filter))
                return filter;


            return new EntitiesFilter(this, include, exclude);
        }

     
        public void RegisterEntityFilter(EntitiesFilter filter)
        {
            var key = filter.GetHashCode();
            if (entitiesFilters.ContainsKey(key))
                throw new Exception("we alrdy have filter with this key, probably u should use getfilter on the world, instead of manualy creating");

            entitiesFilters.Add(key, filter);

            var pool = HECSPooledArray<int>.GetArray(Entities.Length);
            
            for (int i = 0; i < Entities.Length; i++)
            {
                pool.Items[i] = i;
            }

            filter.UpdateFilter(pool.Items, Entities.Length);
            pool.Release();
        }

        public void ForceUpdateFilter(EntitiesFilter filter)
        {
            filter.UpdateFilter(dirtyEntities.Data, dirtyEntities.Count);
        }

        private void InitStandartEntities()
        {
            GlobalUpdateSystem.FinishUpdate += ProcessDirtyEntities;
        }

        private void ProcessDirtyEntities()
        {
        }

        public ref IEntity PullEntity(string id = "empty")
        {
            if (freeIndices.TryDequeue(out var result))
            {
                if (Entities[result] == null)
                    Entities[result] = new Entity(this,  result, id);


                return ref Entities[result];
            }

            else
                return ref ResizeAndReturnEntity();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterEntity(IEntity entity, bool isAdded)
        {
            if (isAdded)
                ProcessAddedEntity(entity);
            else
                ProcessRemovedEntity(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessAddedEntity(IEntity entity)
        {
            if (entity.Index == -1)
            {
                ref var getEntity = ref PullEntity();
                entity.SetID(getEntity.Index);
                getEntity = entity;
            }

            registerEntity.Add(new Core.RegisterEntity { Entity = entity, IsAdded = true });
            RegisterDirtyEntity(entity.Index);

            foreach (var c in entity.Components)
            {
                var icomponent = componentProvidersByTypeIndex[c].GetIComponent(entity.Index);

                //here we add unity part
                ComponentAdditionalProcessing(icomponent, entity);

                if (icomponent is IInitable initable)
                    initable.Init();
            }

            foreach (var s in entity.GetAllSystems)
            {
                SystemAdditionalProcessing(s, entity);

                if (s is IInitable initable)
                    s.InitSystem();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void ComponentAdditionalProcessing(IComponent component, IEntity owner);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void SystemAdditionalProcessing(ISystem system, IEntity owner);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessRemovedEntity(IEntity entity)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MigrateEntityToWorld(IEntity entity)
        {
            if (entity.World == this)
                return;

            if (entity.World == this)
                return;

            var previousWorld = entity.World;
            var previousIndex = entity.Index;
            var freeIndex = PullEntity();

            entity.SetID(freeIndex.Index);
            Entities[freeIndex.Index] = entity;


        }

        public void RegisterDirtyEntity(int index)
        {
            dirtyEntities.Add(index);
        }

        public int GetEntityFreeIndex()
        {
            if (freeIndices.TryDequeue(out var result))
            {
                return result;
            }

            ResizeEntitiesList();
            return GetEntityFreeIndex();
        }

        private ref IEntity ResizeAndReturnEntity()
        {
            ResizeEntitiesList();
            return ref PullEntity();
        }

        private void ResizeEntitiesList()
        {
            var currentLenght = Entities.Length;
            Array.Resize(ref Entities, currentLenght * 2);

            for (int i = currentLenght; i < Entities.Length; i++)
            {
                if (Entities[i] == null)
                {
                    Entities[i] = new Entity(this, i);
                    freeIndices.Enqueue(i);
                }
            }

            foreach (var p in fastComponentProvidersByTypeIndex)
            {
                p.Value.Resize();
            }
        }

        public void RegisterComponentProvider(ComponentProvider componentProvider)
        {
            componentProvidersByTypeIndex.Add(componentProvider.TypeIndexProvider, componentProvider);
        }

        public ComponentProvider GetComponentProvider(int index) => componentProvidersByTypeIndex[index];


        public void AddEntityListener(IReactEntity reactEntity, bool add)
        {
            reactEntities.AddOrRemoveElement(reactEntity, add);
        }

        public void ReleaseEntity(IEntity entity)
        {
            if (entity.IsAlive)
                entity.Dispose();

            freeIndices.Enqueue(entity.Index);
        }
    }

    public struct RegisterEntity 
    {
        public IEntity Entity;
        public bool IsAdded;
    }
}
