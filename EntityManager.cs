using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public partial class EntityManager : IDisposable
    {
        public const int AllWorld = -1;

        private World[] worlds;
        private static EntityManager Instance;

        public static World[] Worlds => Instance.worlds;
        public static World Default => Instance.worlds[0];
        public static bool IsAlive => Instance != null;

        public EntityManager(int worldsCount = 1)
        {
            worlds = new World[worldsCount];

            for (int i = 0; i < worldsCount; i++)
            {
                worlds[i] = new World(i);
            }

            Instance = this;

            foreach (var world in worlds)
            {
                world.Init();
            }
        }

        /// <summary>
        /// Этот метод цепляется к ивенту закрытия приложения и рассылает его по всем системам, размеченным интерфейсом <code>IOnApplicationQuit</code>
        /// </summary>
        public static void OnApplicationExitInvoke()
        {
            foreach (var world in Worlds)
            foreach (var entity in world.Entities)
            foreach (ISystem system in entity.GetAllSystems)
            {
                if (system is IOnApplicationQuit sys) sys.OnApplicationExit();
            }
        }

        /// <summary>
        /// рассылка команд всем подписчикам в мире
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="world"> здесь мы говорим в какой мир отправить, если индекс -1, то отправляем во все миры </param>
        public static void Command<T>(T command, int world = 0) where T : IGlobalCommand
        {
            if (world == -1)
            {
                foreach (var w in Worlds)
                    w.Command(command);

                return;
            }

            Instance.worlds[world].Command(command);
        }

        public static void GlobalCommand<T>(T command) where T : IGlobalCommand
        {
            foreach (var w in Worlds) 
                w.Command(command);
        }

        /// <summary>
        /// Если нам нужно убедиться что такая ентити существует, или дождаться когда она появиться, 
        /// то мы отправляем команду ожидать появления нужной сущности
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="waitForComponent"></param>
        public static void Command<T>(T command, ref HECSMask waitForComponent, int worldIndex = 0) where T : ICommand, IGlobalCommand 
            => Worlds[worldIndex].Command(command, ref waitForComponent);

        public static void RegisterEntity(IEntity entity, bool add)
        {
            Instance.worlds[entity.WorldId].RegisterEntity(entity, add);
        }

        public static ConcurrencyList<IEntity> Filter(FilterMask include, int worldIndex = 0) => Instance.worlds[worldIndex].Filter(include);
        public static ConcurrencyList<IEntity> Filter(FilterMask include, FilterMask exclude, int worldIndex = 0) => Instance.worlds[worldIndex].Filter(include, exclude);
        public static ConcurrencyList<IEntity> Filter(HECSMask mask, int worldIndex = 0) => Instance.worlds[worldIndex].Filter(new FilterMask(mask));

        
        /// <summary>
        /// возвращаем первую ентити у которой есть необходимые нам компоненты
        /// </summary>
        /// <param name="outEntity"></param>
        /// <param name="componentIDs"></param>
        public static bool TryGetEntityByComponents(out IEntity outEntity, ref HECSMask mask, int worldIndex = 0)
        {
            if (worldIndex == -1)
            {
                foreach (var w in Worlds)
                {
                    if (w.TryGetEntityByComponents(out outEntity, ref mask))
                    {
                        return true;
                    }
                }

                outEntity = null;
                return false;
            }

            var world = Instance.worlds[worldIndex];
            return world.TryGetEntityByComponents(out outEntity, ref mask);
        }

        /// <summary>
        /// на самом деле возвращаем первый попавшийся/закешированный, то что он единственный и неповторимый - на вашей совести
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="worldIndex"></param>
        /// <returns></returns>
        public static T GetSingleSystem<T>(int worldIndex = 0) where T : ISystem => Instance.worlds[worldIndex].GetSingleSystem<T>();
        
        /// <summary>
        /// на самом деле возвращаем первый попавшийся/закешированный, то что он единственный и неповторимый - на вашей совести
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="worldIndex"></param>
        /// <returns></returns>
        public static T GetSingleComponent<T>(int worldIndex = 0) where T : IComponent => Instance.worlds[worldIndex].GetSingleComponent<T>();

        public static bool TryGetEntityByID(Guid entityGuid, out IEntity entity, int worldIndex = 0)
        {
            foreach (var w in Worlds)
            {
                if (w.TryGetEntityByID(entityGuid, out entity))
                    return true;
            }

            entity = default;
            return false;
        }

        public bool TryGetSystemFromEntity<T>(ref HECSMask mask, out T system, int worldIndex =0) where T : ISystem
        {
            var world = Instance.worlds[worldIndex];
            return world.TryGetSystemFromEntity(ref mask, out system);
        }

        public static void AddOrRemoveComponent(IComponent component, bool isAdded)
        {
            Instance.worlds[component.Owner.WorldId].AddOrRemoveComponentEvent(component, isAdded);
        }

        public T GetHECSComponent<T>(ref HECSMask owner, int worldIndex = 0) => worlds[worldIndex].GetHECSComponent<T>(ref owner);

        public bool TryGetComponentFromEntity<T>(out T component, ref HECSMask owner, ref HECSMask neededComponent, int worldIndex) where T : IComponent
            => worlds[worldIndex].TryGetComponentFromEntity(out component, ref owner, ref neededComponent);

        public void Dispose()
        {
            Instance = null;

            for (int i = 0; i < worlds.Length; i++)
            {
                worlds[i].Dispose();
            }

            Array.Clear(worlds, 0, worlds.Length);
        }
    }
}