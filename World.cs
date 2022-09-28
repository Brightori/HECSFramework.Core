using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Systems;

namespace HECSFramework.Core
{
    public sealed partial class World : IAddSingleComponent
    {
        public int Index { get; private set; }

        public GlobalComponentListenersService GlobalComponentListenerService = new GlobalComponentListenersService();

        public GlobalUpdateSystem GlobalUpdateSystem { get; private set; } = new GlobalUpdateSystem();
        private ComponentsService componentsService = new ComponentsService();

        private EntityService entityService = new EntityService();
        private EntityGlobalCommandService commandService = new EntityGlobalCommandService();

        private EntityFilter entityFilter;

        private ConcurrentDictionary<HECSMask, IEntity> cacheTryGet = new ConcurrentDictionary<HECSMask, IEntity>();
        private ConcurrentDictionary<Guid, IEntity> cacheTryGetbyGuid = new ConcurrentDictionary<Guid, IEntity>();

        private Dictionary<Type, ISystem> singleSystems = new Dictionary<Type, ISystem>();
        private Dictionary<int, IComponent> singleComponents = new Dictionary<int, IComponent>(16);

        private WaitingCommandsSystems waitingCommandsSystems;

        private Queue<IEntity> waintingForInit = new Queue<IEntity>();

        public bool IsAlive { get; private set; } = true;
        public Guid WorldGuid { get; private set; } = Guid.NewGuid();
        public bool IsInited { get; private set; }

        public World(int index)
        {
            Index = index;
            entityFilter = new EntityFilter(this);
        }

        public void Init()
        {
            if (IsInited)
                return;

            var worldService = new Entity("WorldService", Index);
            waitingCommandsSystems = new WaitingCommandsSystems();
            worldService.AddHecsSystem(waitingCommandsSystems);
            worldService.AddHecsSystem(new DestroyEntityWorldSystem());
            worldService.AddHecsSystem(new RemoveComponentWorldSystem());
            worldService.AddHecsSystem(new PoolingSystem());
            worldService.Init();

            while (waintingForInit.Count > 0)
                waintingForInit.Dequeue().Init();

            IsInited = true;
        }

        public ConcurrencyList<IEntity> Entities => entityService.Entities;
        public int EntitiesCount => Entities.Count;

        public ConcurrencyList<IEntity> Filter(FilterMask include, bool includeAny = false) => entityFilter.GetFilter(include, includeAny);
        public ConcurrencyList<IEntity> Filter(FilterMask include, FilterMask exclude, bool includeAny = false, bool excludeAny = true) => entityFilter.GetFilter(include, exclude, includeAny, excludeAny);
        public ConcurrencyList<IEntity> Filter(HECSMask mask, bool includeAny = false) => entityFilter.GetFilter(new FilterMask(mask), includeAny);


        public void AddToInit(IEntity entity)
        {
            waintingForInit.Enqueue(entity);
        }

        public void AddOrRemoveComponent<T>(T component, bool isAdded) where T : IComponent
        {
            componentsService.ProcessComponent(component, isAdded);
        }

        public void RegisterUpdatable<T>(T registerUpdatable, bool add) where T : IRegisterUpdatable
        {
            GlobalUpdateSystem.Register(registerUpdatable, add);
        }

        public void RegisterEntity(IEntity entity, bool isAdded)
        {
            if (isAdded && entity.GUID == Guid.Empty) entity.GenerateGuid();
            entityService.RegisterEntity(entity, isAdded);
        }

        public void AddEntityListener(IReactEntity reactEntity, bool add)
        {
            entityService.AddEntityListener(reactEntity, add);
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
            commandService.Invoke(command);
        }

        /// <summary>
        /// Если нам нужно убедиться что такая ентити существует, или дождаться когда она появиться, 
        /// то мы отправляем команду ожидать появления нужной сущности
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="waitForComponent"></param>
        public void Command<T>(T command, ref HECSMask waitForComponent) where T : struct, ICommand, IGlobalCommand
        {

            if (TryGetEntityByComponents(out var entity, ref waitForComponent))
            {
                commandService.Invoke(command);
                return;
            }

            waitingCommandsSystems.AddWaitingCommand(command, waitForComponent);
        }

        public void AddGlobalReactCommand<T>(ISystem system, IReactGlobalCommand<T> react) where T : struct, IGlobalCommand
        {
            commandService.AddListener(system, react);
        }

        public void RemoveGlobalReactCommand(ISystem system)
        {
            commandService.ReleaseListener(system);
        }

        public void RemoveGlobalReactCommand<T>(ISystem system) where T : struct, IGlobalCommand
        {
            commandService.RemoveListener<T>(system);
        }

        public void AddGlobalReactComponent(IReactComponent reactComponent)
        {
            componentsService.AddListener(reactComponent);
        }
        public void AddGlobalReactComponent<T>(ISystem system, IReactComponentGlobal<T> action) where T : IComponent
        {
            GlobalComponentListenerService.AddListener(system, action);
        }

        public void RemoveGlobalReactComponent(IReactComponent reactComponent)
        {
            componentsService.RemoveListener(reactComponent);
        }

        public void RemoveGlobalReactComponent<T>(ISystem system) where T : IComponent
        {
            GlobalComponentListenerService.RemoveListener<T>(system);
        }

        /// <summary>
        /// возвращаем первую ентити у которой есть необходимые нам компоненты
        /// </summary>
        /// <param name="outEntity"></param>
        /// <param name="componentIDs"></param>
        public bool TryGetEntityByComponents(out IEntity outEntity, ref HECSMask mask)
        {
            if (cacheTryGet.TryGetValue(mask, out outEntity))
            {
                if (outEntity.IsAlive)
                    return true;
                else
                    cacheTryGet.TryRemove(mask, out var entity);
            }

            var count = EntitiesCount;

            for (int i = 0; i < count; i++)
            {
                var currentEntity = Entities.Data[i];

                if (currentEntity.ContainsMask(ref mask))
                {
                    outEntity = currentEntity;
                    cacheTryGet.TryRemove(mask, out var entity);
                    return true;
                }
            }

            outEntity = null;
            return false;
        }

        /// <summary>
        /// поиск ентити по условиям
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public IEntity GetEntity(Func<IEntity, bool> func)
        {
            foreach (var entity in entityService.Entities)
            {
                if (func(entity))
                    return entity;
            }
            return default;
        }

        /// <summary>
        /// если нам нужно получить систему с одной из сущностей основной логики, 
        /// или любой сущности которая имеет уникальный компонент
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tagOfEntity">уникальный тэг для энтити</param>
        /// <param name="system">тип системы</param>
        /// <returns></returns>
        public bool TryGetSystemFromEntity<T>(ref HECSMask mask, out T system) where T : ISystem
        {
            if (TryGetEntityByComponents(out var entity, ref mask))
            {
                if (entity.TryGetSystem(out system))
                    return true;
            }

            system = default;
            return false;
        }

        public T GetSingleSystem<T>() where T : ISystem
        {
            var key = typeof(T);

            if (singleSystems.TryGetValue(key, out var system))
            {
                if (system.Owner.IsAlive)
                    return (T)system;
            }

            for (int i = 0; i < entityService.Entities.Count; i++)
            {
                if (entityService.Entities.Data[i].TryGetSystem(out T needed))
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

        public T GetSingleComponent<T>() where T : IComponent, IWorldSingleComponent
        {
            var key = TypesMap.GetComponentInfo<T>().ComponentsMask.TypeHashCode;

            if (singleComponents.TryGetValue(key, out var component))
            {
                if (component != null && component.Owner.IsAlive && component.IsAlive)
                    return (T)component;
            }
           
            return default;
        }

        public T GetSingleComponent<T>(HECSMask mask) where T : IComponent, IWorldSingleComponent
        {
            var key = mask.TypeHashCode;

            if (singleComponents.TryGetValue(key, out var component))
            {
                if (component != null && component.Owner.IsAlive && component.IsAlive)
                    return (T)component;
            }

            return default;
        }

        public bool TryGetSingleComponent<T>(out T component) where T: IComponent, IWorldSingleComponent
        {
            var key = TypesMap.GetComponentInfo<T>().ComponentsMask.TypeHashCode;
            component = default;

            if (singleComponents.TryGetValue(key, out var lookForComponent))
            {
                if (lookForComponent != null && lookForComponent.Owner.IsAlive && lookForComponent.IsAlive)
                {
                    component = (T)lookForComponent;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetSingleComponent<T>(HECSMask mask, out T component) where T : IComponent, IWorldSingleComponent
        {
            var key = mask.TypeHashCode;
            component = default;

            if (singleComponents.TryGetValue(key, out var lookForComponent))
            {
                if (lookForComponent != null && lookForComponent.Owner.IsAlive && lookForComponent.IsAlive)
                {
                    component = (T)lookForComponent;
                    return true;
                }
            }

            return false;
        }

        public bool IsHaveSingleComponent(int index)
        {
            return singleComponents.ContainsKey(index);
        }

        public bool TryGetComponentFromEntity<T>(out T component, ref HECSMask owner, ref HECSMask neededComponent) where T : IComponent
        {
            if (TryGetEntityByComponents(out var entity, ref owner))
            {
                component = entity.GetHECSComponent<T>();
                return true;
            }

            component = default;
            return false;
        }

        public bool TryGetEntityByID(Guid entityGuid, out IEntity entity)
        {
            if (cacheTryGetbyGuid.TryGetValue(entityGuid, out entity))
            {
                if (entity.IsAlive())
                    return true;
                else
                    cacheTryGetbyGuid.TryRemove(entityGuid, out var entityOut);
            }
            
            entity = null;
            var entities = entityService.Entities;

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities.Data[i].GUID == entityGuid)
                {
                    cacheTryGetbyGuid.TryAdd(entityGuid, entities.Data[i]);
                    entity = entities.Data[i];
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
            entityService.Dispose();
            componentsService.Dispose();
            commandService.Dispose();
            entityFilter.Dispose();
            GlobalUpdateSystem.Dispose();
            IsAlive = false;
        }

        void IAddSingleComponent.AddSingleWorldComponent<T>(T component, bool add)
        {
            var key = component.GetTypeHashCode;

            if (singleComponents.ContainsKey(key))
            {
                if (add)
                {
                    HECSDebug.LogError("We alrdy have this key|component at singles " + key);
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
            return HashCode.Combine(WorldGuid);
        }
    }

    internal interface IAddSingleComponent
    {
        void AddSingleWorldComponent<T>(T component, bool add) where T : IComponent;
    }
}