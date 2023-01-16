using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Systems;

namespace HECSFramework.Core
{
    public sealed partial class World 
    {
        public int Index { get; private set; }

        public GlobalComponentListenersService GlobalComponentListenerService = new GlobalComponentListenersService();

        public GlobalUpdateSystem GlobalUpdateSystem { get; private set; } = new GlobalUpdateSystem();
        private ComponentsService componentsService = new ComponentsService();

        private EntityGlobalCommandService commandService = new EntityGlobalCommandService();

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
            InitStandartEntities();
            InitFastWorld();
        }


        partial void InitFastWorld();

        public void Init()
        {
            if (IsInited)
                return;

            var worldService = new Entity(this, "WorldService");
            waitingCommandsSystems = new WaitingCommandsSystems();
            worldService.AddHecsSystem(waitingCommandsSystems);
            worldService.AddHecsSystem(new DestroyEntityWorldSystem());
            worldService.AddHecsSystem(new RemoveComponentWorldSystem());
            worldService.AddHecsSystem(new PoolingSystem());
            worldService.Init();

            while (waintingForInit.Count > 0)
                waintingForInit.Dequeue().Init(this);

            IsInited = true;
        }

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
       
        public IEntity GetEntity(Func<IEntity, bool> func)
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
                if (system.Owner.IsAlive)
                    return (T)system;
            }

            for (int i = 0; i < Entities.Length; i++)
            {
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
            componentsService.Dispose();
            commandService.Dispose();
            GlobalUpdateSystem.Dispose();
            FastWorldDispose();
            IsAlive = false;
        }

        partial void FastWorldDispose();

        public void AddSingleWorldComponent<T>(T component, bool add) where T : IComponent, IWorldSingleComponent, new() 
        {
            var key = ComponentProvider<T>.TypeIndex;

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
            return WorldGuid.GetHashCode();
        }
    }
}