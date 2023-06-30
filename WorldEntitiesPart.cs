using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Helpers;

namespace HECSFramework.Core
{
    public partial class World
    {
        public const int StartEntitiesCount = 32;

        public Entity[] Entities;
        private HECSList<IReactEntity> reactEntities = new HECSList<IReactEntity>(32);

        //here we have free entities for pooling|using
        private Stack<int> freeIndicesForStandartEntities = new Stack<int>();

        //dirty entities should be processing 
        private HECSList<int> dirtyEntities = new HECSList<int>(32);
        private HECSList<RegisterEntity> registerEntity = new HECSList<RegisterEntity>(32);

        private Dictionary<int, ComponentProvider> componentProvidersByTypeIndex = new Dictionary<int, ComponentProvider>(128);
        private Dictionary<int, List<EntitiesFilter>> entitiesFilters = new Dictionary<int, List<EntitiesFilter>>(8);

        private ComponentProviderRegistrator[] componentProviderRegistrators;

        public EntitiesFilter GetFilter<T>() where T : IComponent, new() => GetFilterFromCache(Filter.Get<T>(), new Filter());
        public EntitiesFilter GetFilter(Filter include) => GetFilterFromCache(include, new Filter());
        public EntitiesFilter GetFilter(Filter inclide, Filter exclude) => GetFilterFromCache(inclide, exclude);

        public IEnumerable<ComponentProvider> ComponentProviders => componentProvidersByTypeIndex.Values;

        private SystemRegisterService systemRegisterService = new SystemRegisterService();
        private Dictionary<int, Stack<ISystem>> systemsPool = new Dictionary<int, Stack<ISystem>>(32);

        private EntitiesFilter GetFilterFromCache(Filter include, Filter exclude)
        {
            var includeHash = include.GetHashCode();
            var excludeHash = exclude.GetHashCode();
            var key = includeHash + excludeHash;

            if (entitiesFilters.TryGetValue(key, out var listFilters))
            {
                for (int i = 0; i < listFilters.Count; i++)
                {
                    var filter = listFilters[i];
                    if (filter.IncludeHash.Equals(includeHash) && filter.ExcludeHash.Equals(excludeHash))
                    {
                        return filter;
                    }
                }
            }

            return new EntitiesFilter(this, include, exclude);
        }

        partial void FillRegistrators();

        public void RegisterEntityFilter(EntitiesFilter filter)
        {
            var key = filter.GetHashCode();
            if (!entitiesFilters.ContainsKey(key))
            {
                entitiesFilters.Add(key, new List<EntitiesFilter>(2));
            }
            else
            {
                foreach (var f in entitiesFilters[key])
                {
                    if (f.IncludeHash.Equals(filter.IncludeHash) && f.ExcludeHash.Equals(filter.ExcludeHash))
                    {
                        throw new Exception("we alrdy have filter with this key, probably u should use getfilter on the world, instead of manualy creating");
                    }
                }
            }

            entitiesFilters[key].Add(filter);

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

        public void ForceComponentsReact<T>() where T : IComponent
        {
            ComponentProvider<T>.ComponentsToWorld.Data[Index].ForceReact();
        }

        private void InitStandartEntities()
        {
            Entities = new Entity[StartEntitiesCount];

            for (int i = 0; i < Entities.Length; i++)
            {
                Entities[i] = new Entity(i, this);
                Entities[i].IsRegistered = true;

                if (i == 0)
                    continue;

                freeIndicesForStandartEntities.Push(i);
            }

            GlobalUpdateSystem.FinishUpdate += ProcessDirtyEntities;
        }

        private void ProcessDirtyEntities()
        {
            var registerCount = registerEntity.Count;
            var reactEntityCount = reactEntities.Count;

            for (int i = 0; i < registerCount; i++)
            {
                for (int z = 0; z < reactEntityCount; z++)
                {
                    var data = registerEntity.Data[i];
                    reactEntities.Data[z].EntityReact(data.Entity, data.IsAdded);
                }
            }

            if (dirtyEntities.Count > 0)
            {
                foreach (var list in entitiesFilters)
                {
                    foreach (var f in list.Value)
                    {
                        f.UpdateFilter(dirtyEntities.Data, dirtyEntities.Count);
                    }
                }
            }

            for (int i = 0; i < dirtyEntities.Count; i++)
            {
                Entities[dirtyEntities.Data[i]].IsDirty = false;
            }

            for (int i = 0; i < registerCount; i++)
            {
                var register = registerEntity.Data[i];

                if (!register.IsAdded)
                    freeIndicesForStandartEntities.Push(register.Entity.Index);
            }

            dirtyEntities.Clear();
            registerEntity.Clear();
        }

        public Entity GetEntityFromPool(string id = "empty")
        {
            if (freeIndicesForStandartEntities.TryPop(out var result))
            {
                if (Entities[result] == null)
                    Entities[result] = new Entity(result, this, id);

                Entities[result].ID = id;
                Entities[result].IsInited = false;
                Entities[result].IsDisposed = false;
                Entities[result].IsAlive = true;
                Entities[result].IsRegistered = true;
                Entities[result].Generation++;

                return Entities[result];
            }

            else
                return ResizeAndReturnEntity(id);
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
            if (!entity.IsRegistered)
            {
                if (Entities[entity.Index] != null && Entities[entity.Index].IsAlive)
                    entity.Index = GetEntityFreeIndex();

                Entities[entity.Index] = entity;
            }

            entity.IsDisposed = false;
            entity.IsRegistered = true;
            registerEntity.Add(new Core.RegisterEntity { Entity = entity, IsAdded = true });

            foreach (var c in entity.Components)
            {
                var componentProvider = componentProvidersByTypeIndex[c];
                var icomponent = componentProvider.GetIComponent(entity.Index);

                //here we add unity part
                ComponentAdditionalProcessing(icomponent, entity, true);

                if (icomponent is IInitable initable)
                    initable.Init();

                if (icomponent is IWorldSingleComponent singleComponent)
                    this.AddSingleWorldComponent(singleComponent, true);
            }

            foreach (var s in entity.Systems)
                TypesMap.BindSystem(s);

            foreach (var s in entity.Systems)
            {
                SystemAdditionalProcessing(s, entity, true);
                s.InitSystem();
            }

            foreach (var c in entity.Components)
            {
                var componentProvider = componentProvidersByTypeIndex[c];
                componentProvider.RegisterReactive(entity.Index, true);
            }

            
            entity.IsInited = true;

            foreach (var c in entity.Components)
            {
                var icomponent = componentProvidersByTypeIndex[c].GetIComponent(entity.Index);

                if (icomponent is IAfterEntityInit initable)
                    initable.AfterEntityInit();
            }

            foreach (var s in entity.Systems)
            {
                if (s is IAfterEntityInit initable)
                    initable.AfterEntityInit();

                RegisterSystem(s);
            }

            RegisterDirtyEntity(entity.Index);
        }

        public void AdditionalProcessing(IComponent component, Entity owner, bool add)
        {
            ComponentAdditionalProcessing(component, owner, add);
        }

        public void AdditionalProcessing(ISystem system, Entity owner, bool add)
        {
            SystemAdditionalProcessing(system, owner, add);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void ComponentAdditionalProcessing(IComponent component, Entity owner, bool add);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void SystemAdditionalProcessing(ISystem system, Entity owner, bool add);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessRemovedEntity(Entity entity)
        {

            entity.IsAlive = false;
            entity.IsInited = false;
            entity.IsPaused = false;
            entity.IsDirty = false;

            RegisterDirtyEntity(entity.Index);
            registerEntity.Add(new RegisterEntity { Entity = entity, IsAdded = false });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ISystem GetSystemFromPool(int index)
        {
            if (systemsPool.TryGetValue(index, out var systemStack))
            {
                if (systemStack.TryPop(out var system))
                {
                    system.IsDisposed = false;
                    return system;
                }
            }

            return TypesMap.GetSystemFromFactory(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnSystemToPool(int index, ISystem system)
        {
            //this functionality not done yet

            //if (systemsPool.ContainsKey(index))
            //{
            //    if (systemsPool[index].Count > Entities.Length)
            //        return;

            //    systemsPool[index].Push(system);
            //}
            //else
            //{
            //    systemsPool.Add(index, new Stack<ISystem>(16));
            //    systemsPool[index].Push(system);
            //}

            //system.Owner = null;
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

        /// <summary>
        /// we should use this api when we dont need updatable filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<Entity> GetEntitiesByComponent<T>() where T : IComponent, new()
        {
            foreach (var e in Entities)
            {
                if (!e.IsAlive || !e.IsInited)
                    continue;

                if (e.ContainsMask<T>())
                    yield return e;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterDirtyEntity(int index)
        {
            if (Entities[index].IsDirty || !Entities[index].IsInited)
                return;

            Entities[index].IsDirty = true;
            dirtyEntities.Add(index);
        }

        public int GetEntityFreeIndex()
        {
            if (freeIndicesForStandartEntities.TryPop(out var result))
            {
                return result;
            }

            ResizeEntitiesList();
            return GetEntityFreeIndex();
        }

        private Entity ResizeAndReturnEntity(string id)
        {
            ResizeEntitiesList();
            return GetEntityFromPool(id);
        }

        private void ResizeEntitiesList()
        {
            var currentLenght = Entities.Length;
            Array.Resize(ref Entities, currentLenght * 2);

            for (int i = currentLenght; i < Entities.Length; i++)
            {
                if (Entities[i] == null)
                {
                    Entities[i] = new Entity(i, this);
                    freeIndicesForStandartEntities.Push(i);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] GetComponents<T>() where T: IComponent
        {
            return ComponentProvider<T>.ComponentsToWorld.Data[index].Components;
        }

        public void AddEntityListener(IReactEntity reactEntity, bool add)
        {
            if (add)
                reactEntities.Add(reactEntity);
            else
                reactEntities.Remove(reactEntity);
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
