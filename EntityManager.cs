using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public partial class EntityManager : IDisposable
    {
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

        public static void Command<T>(T command, int world = 0) where T : IGlobalCommand
        {
            Instance.worlds[world].Command(command);
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

        public static List<IEntity> Filter(HECSMask include, HECSMask exclude, int worldIndex = 0) => Instance.worlds[worldIndex].Filter(include, exclude);
        public static List<IEntity> Filter(HECSMask include, int worldIndex = 0) => Instance.worlds[worldIndex].Filter(include);

        /// <summary>
        /// возвращаем первую ентити у которой есть необходимые нам компоненты
        /// </summary>
        /// <param name="outEntity"></param>
        /// <param name="componentIDs"></param>
        public static bool TryGetEntityByComponents(out IEntity outEntity, ref HECSMask mask, int worldIndex = 0)
        {
            var world = Instance.worlds[worldIndex];
            return world.TryGetEntityByComponents(out outEntity, ref mask);
        }

        public static bool TryGetEntityByID(Guid entityGuid, out IEntity entity, int worldIndex = 0) 
            => Worlds[worldIndex].TryGetEntityByID(entityGuid, out entity);

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
            for (int i = 0; i < worlds.Length; i++)
            {
                worlds[i].Dispose();
            }

            Array.Clear(worlds, 0, worlds.Length);
        }
    }
}