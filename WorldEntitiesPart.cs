using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HECSFramework.Core.Helpers;
using Helpers;

namespace HECSFramework.Core
{
    public partial class World
    {
        public const int StartEntitiesCount = 32;

        public Entity[] Entities;
        private HECSList<IReactEntity> reactEntities = new HECSList<IReactEntity>();

        //here we have free entities for pooling|using
        private Queue<int> freeIndices = new Queue<int>();

        //dirty entities should be processing 
        private HECSList<int> dirtyEntities = new HECSList<int>(32);
        private HECSList<RegisterEntity> registerEntity = new HECSList<RegisterEntity>(32);

        private Dictionary<int, ComponentProvider> componentProvidersByTypeIndex = new Dictionary<int, ComponentProvider>(256);
        private Dictionary<int, EntitiesFilter> entitiesFilters = new Dictionary<int, EntitiesFilter>(8);

        private ComponentProviderRegistrator[] componentProviderRegistrators;

        public EntitiesFilter GetFilter<T>() where T : IComponent, new() => GetFilterFromCache(Filter.Get<T>(), new Filter());
        public EntitiesFilter GetFilter(Filter include) => GetFilterFromCache(include, new Filter());
        public EntitiesFilter GetFilter(Filter inclide, Filter exclude) => GetFilterFromCache(inclide, exclude);

        public IEnumerable<ComponentProvider> ComponentProviders => componentProvidersByTypeIndex.Values;

        private SystemRegisterService systemRegisterService = new SystemRegisterService();
        private Dictionary<int, Stack<ISystem>> systemsPool = new Dictionary<int, Stack<ISystem>>(32);

        private EntitiesFilter GetFilterFromCache(Filter include, Filter exclude)
        {
            var key = include.GetHashCode() + exclude.GetHashCode();

            if (entitiesFilters.TryGetValue(key, out var filter))
                return filter;

            return new EntitiesFilter(this, include, exclude);
        }

        partial void FillStandartComponentRegistrators();

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

        public void ForceComponentsReact<T>() where T: IComponent
        {
            ComponentProvider<T>.ComponentsToWorld.Data[Index].ForceReact();
        }

        private void InitStandartEntities()
        {
            Entities = new Entity[StartEntitiesCount];

            for (int i = 0; i < Entities.Length; i++)
            {
                Entities[i] = new Entity(i, this);
                freeIndices.Enqueue(i);
            }

            GlobalUpdateSystem.FinishUpdate += ProcessDirtyEntities;
        }

        private void ProcessDirtyEntities()
        {
            foreach (var r in registerEntity)
            {
                foreach (var react in reactEntities)
                    react.EntityReact(r.Entity, r.IsAdded);
            }

            foreach (var f in entitiesFilters)
                f.Value.UpdateFilter(dirtyEntities.Data, dirtyEntities.Count);

            foreach (var e in dirtyEntities)
                Entities[e].IsDirty = false;

            dirtyEntities.Clear();
            registerEntity.Clear();
        }

        public ref Entity GetEntityFromPool(string id = "empty")
        {
            if (freeIndices.TryDequeue(out var result))
            {
                if (Entities[result] == null)
                    Entities[result] = new Entity (result, this, id);

                Entities[result].IsInited = false;
                Entities[result].IsAlive = true;

                return ref Entities[result];
            }

            else
                return ref ResizeAndReturnEntity();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterEntity(Entity entity, bool isAdded)
        {
            if (isAdded)
                ProcessAddedEntity(entity);
            else
                ProcessRemovedEntity(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessAddedEntity(Entity entity)
        {
            if (entity.Index == -1)
            {
                ref var getEntity = ref GetEntityFromPool();
                entity.SetID(getEntity.ID);
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

            foreach (var s in entity.Systems)
                TypesMap.BindSystem(s);

            foreach (var s in entity.Systems)
            {
                SystemAdditionalProcessing(s, entity);
                s.InitSystem();
            }

            foreach (var c in entity.Components)
            {
                var icomponent = componentProvidersByTypeIndex[c].GetIComponent(entity.Index);

                if (icomponent is IAfterEntityInit initable)
                    initable.AfterEntityInit();
            }

            foreach (var s in entity.Systems)
            {
                SystemAdditionalProcessing(s, entity);

                if (s is IAfterEntityInit initable)
                    initable.AfterEntityInit();

                RegisterSystem(s);
            }

            entity.IsInited = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void ComponentAdditionalProcessing(IComponent component, Entity owner);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void SystemAdditionalProcessing(ISystem system, Entity owner);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessRemovedEntity(Entity entity)
        {
            freeIndices.Enqueue(entity.Index);
            entity.IsAlive = false;
            entity.IsInited = false;
            entity.IsPaused = false;
            entity.IsDirty = false;

            RegisterDirtyEntity(entity.Index);
            registerEntity.Add(new RegisterEntity { Entity = entity, IsAdded = false });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public System GetSystemFromPool<System>(int index) where System : class, ISystem, new()
        {
            if (systemsPool.TryGetValue(index, out var systemStack))
            {
                if (systemStack.TryPop(out var system))
                    return (System)system;
            }

            return new System();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnSystemToPool(int index, ISystem system)
        {
            if (systemsPool.ContainsKey(index))
            {
                if (systemsPool[index].Count > Entities.Length)
                    return;

                systemsPool[index].Push(system);
            }
            else
            {
                systemsPool.Add(index, new Stack<ISystem>(16));
                systemsPool[index].Push(system);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MigrateEntityToWorld(Entity entity)
        {
            if (entity.World == this)
                return;

            var previousWorld = entity.World;
            var previousIndex = entity.Index;
            var freeIndex = GetEntityFromPool();

            if (entity.IsInited)
            {
                foreach (var c in entity.Components)
                {
                    var component = previousWorld.GetComponentProvider(c).GetIComponent(previousIndex);

                    if (component is IBeforeMigrationToWorld before)
                        before.BeforeMigration();
                }

                foreach (var s in entity.Systems)
                {
                    if (s is IBeforeMigrationToWorld before)
                        before.BeforeMigration();

                    UnRegisterSystem(s);
                }
            }

            entity.Index = freeIndex.Index;
            Entities[freeIndex.Index] = entity;
            entity.World = this;

            foreach (var c in entity.Components)
                SwapComponents(c, previousIndex, previousWorld, entity.Index, this);

            if (entity.IsInited)
            {
                foreach (var c in entity.Components)
                {
                    var component = previousWorld.GetComponentProvider(c).GetIComponent(previousIndex);

                    if (component is IAfterMigrationToWorld before)
                        before.AfterMigration();
                }

                foreach (var s in entity.Systems)
                {
                    if (s is IAfterMigrationToWorld before)
                        before.AfterMigration();

                    TypesMap.BindSystem(s);
                    RegisterSystem(s);
                }
            }

            RegisterDirtyEntity(entity.Index);
            registerEntity.Add(new Core.RegisterEntity { Entity = entity, IsAdded = true });
            previousWorld.RegisterDirtyEntity(previousIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SwapComponents(int indexComponent, int indexFromEntity, World fromWorld, int indexToEntity, World toWorld)
        {
            var componentProvider = toWorld.GetComponentProvider(indexComponent);
            var temp = componentProvider.GetIComponent(indexToEntity);
            
            fromWorld.GetComponentProvider(indexComponent).RegisterComponent(indexFromEntity, false);

            toWorld.GetComponentProvider(indexComponent)
                .SetIComponent(indexToEntity, fromWorld.GetComponentProvider(indexComponent).GetIComponent(indexFromEntity));

            fromWorld.GetComponentProvider(indexComponent)
                .SetIComponent(indexToEntity, temp);

            toWorld.GetComponentProvider(indexComponent).RegisterComponent(indexToEntity, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterDirtyEntity(int index)
        {
            if (Entities[index].IsDirty)
                return;

            Entities[index].IsDirty = true;
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

        private ref Entity ResizeAndReturnEntity()
        {
            ResizeEntitiesList();
            return ref GetEntityFromPool();
        }

        private void ResizeEntitiesList()
        {
            var currentLenght = Entities.Length;
            Array.Resize(ref Entities, currentLenght * 2);

            for (int i = currentLenght; i < Entities.Length; i++)
            {
                if (Entities[i] == null)
                {
                    Entities[i] = new Entity (i, this);
                    freeIndices.Enqueue(i);
                }
            }

            foreach (var p in componentProvidersByTypeIndex)
            {
                p.Value.Resize();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterComponentProvider(ComponentProvider componentProvider)
        {
            componentProvidersByTypeIndex.Add(componentProvider.TypeIndexProvider, componentProvider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentProvider GetComponentProvider(int index) => componentProvidersByTypeIndex[index];


        public void AddEntityListener(IReactEntity reactEntity, bool add)
        {
            reactEntities.AddOrRemoveElement(reactEntity, add);
        }

        public void ReleaseEntity(Entity entity)
        {
            if (entity.IsAlive)
                entity.Dispose();

            freeIndices.Enqueue(entity.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RegisterSystem<T>(T system) where T : ISystem
        {
            systemRegisterService.RegisterSystem(system);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnRegisterSystem(ISystem system)
        {
            systemRegisterService.UnRegisterSystem(system);
        }
    }

    public struct RegisterEntity
    {
        public Entity Entity;
        public bool IsAdded;
    }
}
