using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Components;
using Systems;

namespace HECSFramework.Core
{
    public sealed partial class World
    {
        private int index;
        public int Index { get => index; private set => index = value; }

        public GlobalUpdateSystem GlobalUpdateSystem { get; private set; } = new GlobalUpdateSystem();
        private ComponentsService componentsService;

        private ConcurrentDictionary<Guid, Entity> cacheTryGetbyGuid = new ConcurrentDictionary<Guid, Entity>();

        private Dictionary<Type, ISystem> singleSystems = new Dictionary<Type, ISystem>();
        private Dictionary<int, IComponent> singleComponents = new Dictionary<int, IComponent>(16);

        private WaitingCommandsSystems waitingCommandsSystems;

        private Queue<Entity> waintingForInit = new Queue<Entity>();

        public bool IsAlive { get; private set; } = true;
        public Guid WorldGuid { get; private set; } = Guid.NewGuid();
        public bool IsInited { get; private set; }

        public World(int index)
        {
            Index = index;
            FillRegistrators();

            foreach (var tr in componentProviderRegistrators)
                tr.RegisterWorld(this);

            componentsService = new ComponentsService(this);
            InitStandartEntities();
            InitFastWorld();
        }

        partial void InitFastWorld();

        public void Init()
        {
            if (IsInited)
                return;

            var worldService = Entity.Get(this, "WorldService");
            worldService.AddComponent<CachedEntitiesGlobalHolderComponent>();

            waitingCommandsSystems = new WaitingCommandsSystems();
            worldService.AddHecsSystem(waitingCommandsSystems);
            worldService.AddHecsSystem(new AwaitersUpdateSystem());
            worldService.AddHecsSystem(new DestroyEntityWorldSystem());
            worldService.AddHecsSystem(new RemoveComponentWorldSystem());
            worldService.AddHecsSystem(new PoolingSystem());
            AddUnityWorldPart(worldService);
            worldService.Init();
            AddStrategiesPart();
            while (waintingForInit.Count > 0)
                waintingForInit.Dequeue().Init();

            IsInited = true;
        }

        partial void AddUnityWorldPart(Entity worldService);
        partial void AddStrategiesPart();

        public void AddToInit(Entity entity)
        {
            waintingForInit.Enqueue(entity);
        }

        public void RegisterUpdatable<T>(T registerUpdatable, bool add) where T : IRegisterUpdatable
        {
            GlobalUpdateSystem.Register(registerUpdatable, add);
        }

        /// <summary>
        /// Рассылает команды по дефолту только  тем ентити у которых зарегестрированы глобальные системы, 
        /// можно рассылать всем подряд.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="isGlobalOnly"></param>
        public void Command<T>(T command) where T : struct, ICommand, IGlobalCommand
        {
            if (index < GlobalCommandListener<T>.ListenersToWorld.Count)
                GlobalCommandListener<T>.ListenersToWorld.Data[index]?.Invoke(command);
        }

        public void AddGlobalReactCommand<T>(ISystem system, IReactGlobalCommand<T> react) where T : struct, IGlobalCommand
        {
            GlobalCommandListener<T>.AddListener(Index, react);
        }

        public void RemoveGlobalReactCommand<T>(ISystem system) where T : struct, IGlobalCommand
        {
            GlobalCommandListener<T>.RemoveListener(Index, system);
        }

        public void AddGlobalGenericReactComponent<T>(IReactGenericGlobalComponent<T> reactComponent, bool added)
        {
            componentsService.AddGenericListener(reactComponent, added);
        }

        public void AddLocalGenericReactComponent<T>(int index, IReactGenericLocalComponent<T> reactComponent, bool added)
        {
            componentsService.AddLocalGenericListener(index, reactComponent, added);
        }

        public void AddGlobalReactComponent<T>(IReactComponentGlobal<T> action, bool added) where T : IComponent
        {
            componentsService.AddListener(action, added);
        }

        public void AddLocalReactComponent<T>(int entity, IReactComponentLocal<T> action, bool add) where T : IComponent
        {
            componentsService.AddLocalListener(entity, action, add);
        }

        public Entity GetEntity(Func<Entity, bool> func)
        {
            foreach (var entity in Entities)
            {
                if (func(entity))
                    return entity;
            }
            return default;
        }

        public T GetSingleSystem<T>() where T : ISystem
        {
            var key = typeof(T);

            if (singleSystems.TryGetValue(key, out var system))
            {
                if (system != null && system.Owner != null && system.Owner.IsAlive)
                    return (T)system;
            }

            for (int i = 0; i < Entities.Length; i++)
            {
                if (!Entities[i].IsAlive)
                    continue;

                if (Entities[i].TryGetSystem(out T needed))
                {
                    if (needed.Owner.IsAlive)
                    {
                        if (singleSystems.ContainsKey(key))
                        {
                            singleSystems[key] = needed;
                            return needed;
                        }

                        singleSystems.Add(typeof(T), needed);
                        return needed;
                    }
                }
            }

            return default;
        }

        public T GetSingleComponent<T>() where T : IComponent, IWorldSingleComponent, new()
        {
            var key = ComponentProvider<T>.TypeIndex;

            if (singleComponents.TryGetValue(key, out var component))
            {
                if (component != null && component.Owner.IsAlive && component.IsAlive)
                    return (T)component;
            }

            return default;
        }

        public bool TryGetSingleComponent<T>(out T component) where T : IComponent, IWorldSingleComponent
        {
            var key = ComponentProvider<T>.TypeIndex;

            if (singleComponents.TryGetValue(key, out var lookForComponent))
            {
                if (lookForComponent != null && lookForComponent.Owner.IsAlive && lookForComponent.IsAlive)
                {
                    component = (T)lookForComponent;
                    return true;
                }
            }

            component = default;
            return false;
        }

        public bool IsHaveSingleComponent(int index)
        {
            return singleComponents.ContainsKey(index);
        }

        public bool IsHaveSingleComponent<T>() where T: IComponent, IWorldSingleComponent
        {
            return singleComponents.ContainsKey(ComponentProvider<T>.TypeIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity GetEntityBySingleComponent(int index)
        {
            return singleComponents[index].Owner;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity GetEntityBySingleComponent<T>() where T : IComponent, IWorldSingleComponent
        {
            return singleComponents[ComponentProvider<T>.TypeIndex].Owner;
        }

        public bool TryGetEntityByComponent<T>(out Entity outEntity) where T : IComponent, new()
        {
            var provider = ComponentProvider<T>.ComponentsToWorld.Data[Index];

            for (int i = 0; i < provider.Components.Length; i++)
            {
                if (provider.Components[i] != null && provider.Components[i].IsAlive)
                {
                    outEntity = provider.Components[i].Owner;
                    return true;
                }
            }

            outEntity = null;
            return false;
        }

        public bool TryGetSystemFromEntity<T>(int ComponentTypeIndex, out T system) where T : ISystem
        {
            foreach (var e in Entities)
            {
                if (e.Components.Contains(ComponentTypeIndex))
                {
                    if (e.TryGetSystem(out system))
                        return true;
                }
            }

            system = default;
            return false;
        }

        public bool TryGetEntityByID(Guid entityGuid, out Entity entity)
        {
            if (cacheTryGetbyGuid.TryGetValue(entityGuid, out entity))
            {
                if (entity.IsAlive())
                    return true;
                else
                    cacheTryGetbyGuid.TryRemove(entityGuid, out var entityOut);
            }

            entity = null;

            for (int i = 0; i < Entities.Length; i++)
            {
                if (Entities[i].GUID == entityGuid)
                {
                    cacheTryGetbyGuid.TryAdd(entityGuid, Entities[i]);
                    entity = Entities[i];
                    break;
                }
            }

            return entity != null;
        }

        public void UpdateIndex(int index)
        {
            Index = index;
        }

        public void Dispose()
        {
            for (int i = 0; i < Entities.Length; i++)
            {
                if (Entities[i].IsAlive)
                    Entities[i].Dispose();
            }

            componentsService.Dispose();
            GlobalUpdateSystem.Dispose();
            FastWorldDispose();

            singleSystems.Clear();
            singleComponents.Clear();
            freeEntities.Clear();
            dirtyEntities.Clear();

            foreach (var c in componentProvidersByTypeIndex.Values)
                c.Dispose();
            componentProvidersByTypeIndex.Clear();


            reactEntities.Clear();
            entitiesFilters.Clear();

            systemRegisterService = null;
            componentProvidersByTypeIndex.Clear();
            componentProviderRegistrators = null;

            foreach (var pool in systemsPool.Values)
                pool.Clear();

            systemsPool.Clear();

            Array.Clear(Entities, 0, Entities.Length);
            IsAlive = false;
        }

        partial void FastWorldDispose();

        public void AddSingleWorldComponent<T>(T component, bool add) where T : IComponent, IWorldSingleComponent
        {
            var key = component.GetTypeHashCode;

            if (singleComponents.ContainsKey(key))
            {
                if (add)
                {
                    HECSDebug.LogError("We alrdy have this key|component at singles " + key);
                    HECSDebug.LogError($"We add duplicate to singles from entity: {component.Owner.ID} container: {component.Owner.ContainerID}");
                    return;
                }
                else
                    singleComponents.Remove(key);
            }
            else
            {
                if (add)
                    singleComponents.Add(key, component);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is World world &&
                   WorldGuid.Equals(world.WorldGuid);
        }

        public override int GetHashCode()
        {
            return WorldGuid.GetHashCode();
        }
    }
}