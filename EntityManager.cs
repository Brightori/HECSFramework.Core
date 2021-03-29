using System;

namespace HECSFramework.Core
{
    public class EntityManager : IDisposable
    {
        private World[] worlds;
        private static EntityManager Instance;

        public static World[] Worlds => Instance.worlds;
        public static World Default => Instance.worlds[0];

        public EntityManager(int worldsCount = 1)
        {
            worlds = new World[worldsCount];

            for (int i = 0; i < worldsCount; i++)
            {
                worlds[i] = new World();
            }

            Instance = this;
        }

        public static void Command<T>(T command, int world = 0) where T : IGlobalCommand
        {
            Instance.worlds[world].Command(command);
        }

        public static void RegisterEntity(IEntity entity, bool add)
        {
            Instance.worlds[entity.WorldId].RegisterEntity(entity, add);
        }

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

        public static void AddOrRemoveComponent(IComponent component, bool isAdded)
        {
            Instance.worlds[component.Owner.WorldId].AddOrRemoveComponentEvent(component, isAdded);
        }

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