using System;
using System.Collections.Generic;
using System.Linq;
using Systems;

namespace HECSFramework.Core
{
    public class World
    {
        public int Index { get; private set; }

        private ComponentsService componentsService = new ComponentsService();
        private EntityService entityService = new EntityService();
        private EntityCommandService commandService = new EntityCommandService();
        public GlobalUpdateSystem GlobalUpdateSystem { get; private set; } = new GlobalUpdateSystem();
        private EntityFilter entityFilter;

        private Dictionary<HECSMask, IEntity> cacheTryGet = new Dictionary<HECSMask, IEntity>(6);
        private WaitingCommandsSystems waitingCommandsSystems;

        public World(int index)
        {
            Index = index;
            entityFilter = new EntityFilter(this);
        }

        public void Init()
        {
            var worldService = new Entity("WorldService", Index);
            waitingCommandsSystems = new WaitingCommandsSystems();
            worldService.AddHecsSystem(waitingCommandsSystems);
            worldService.Init();
        }

        public IEntity[] Entities => entityService.Entities;
        public int EntitiesCount => entityService.Count;

        public List<IEntity> Filter(HECSMask include, HECSMask exclude) => entityFilter.GetFilter(include, exclude);
        public List<IEntity> Filter(HECSMask include) => entityFilter.GetFilter(include);

        public void AddOrRemoveComponentEvent(IComponent component, bool isAdded)
        {
            componentsService.ProcessComponent(component, isAdded);
        }

        public void RegisterUpdatable<T>(T registerUpdatable, bool add) where T: IRegisterUpdatable
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
        public void Command<T>(T command) where T : ICommand, IGlobalCommand
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
        public void Command<T>(T command, ref HECSMask waitForComponent) where T : ICommand, IGlobalCommand
        {
            if (TryGetEntityByComponents(out var entity, ref waitForComponent))
            {
                commandService.Invoke(command);
            }

            waitingCommandsSystems.AddWaitingCommand(command, waitForComponent);
        }

        public void AddGlobalReactCommand<T>(ISystem system, Action<T> react) where T: IGlobalCommand
        {
            commandService.AddListener(system, react);
        }
        
        public void RemoveGlobalReactCommand(ISystem system) 
        {
            commandService.ReleaseListener(system);
        }

        public void AddGlobalReactComponent(IReactComponent reactComponent)
        {
            componentsService.AddListener(reactComponent);
        }
        
        public void RemoveGlobalReactComponent(IReactComponent reactComponent)
        {
            componentsService.RemoveListener(reactComponent);
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
                    cacheTryGet.Remove(mask);
            }

            var count = EntitiesCount;

            for (int i = 0; i < count; i++)
            {
                var currentEntity = Entities[i];

                if (currentEntity.ContainsMask(ref mask))
                {
                    outEntity = currentEntity;
                    cacheTryGet.Add(mask, currentEntity);
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

        public T GetHECSComponent<T>(ref HECSMask owner)
        {
            if (TryGetEntityByComponents(out var entity, ref owner))
                return entity.GetHECSComponent<T>();
            else
                return default;
        }

        public bool TryGetComponentFromEntity<T>(out T component, ref HECSMask owner, ref HECSMask neededComponent) where T: IComponent
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
            entity = Entities.FirstOrDefault(a => a.GUID == entityGuid);
            return entity != null;
        }

        public void Dispose()
        {
            componentsService.Dispose();
            entityService.Dispose();
            commandService.Dispose();
            entityFilter.Dispose();
        }
    }
}