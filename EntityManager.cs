using System;
using System.Collections.Generic;
using System.Linq;

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

        public static event Action<World> OnNewWorldAdded;
        public static event Action<World> WorldRemoved;

        private static Queue<int> worldsFreeIndeces;

        public EntityManager(int worldsCount = 1)
        {
            worlds = new World[worldsCount * 2];
            Instance = this;
            worldsFreeIndeces = new Queue<int>(8);

            for (int i = 0; i < worlds.Length; i++)
            {
                worldsFreeIndeces.Enqueue(i);
            }

            for (int i = 0; i < worldsCount; i++)
            {
                AddWorld();
            }

            foreach (var world in worlds)
            {
                world?.Init();
            }
        }

        public static World AddWorld()
        {
            lock (Instance.worlds)
            {
                if (worldsFreeIndeces.TryDequeue(out var freeIndex))
                {
                    return AddWorld(freeIndex);
                }
                else
                {
                    ResizeWorlds();
                    return AddWorld();
                }
            }
        }

        private static void ResizeWorlds() 
        {
            lock (Instance.worlds)
            {
                var lenght = Instance.worlds.Length;
                Array.Resize(ref Instance.worlds, Instance.worlds.Length * 2);

                worldsFreeIndeces.Clear();

                for (int i = 0; i < Instance.worlds.Length; i++)
                {
                    if (Worlds[i] == null)
                        worldsFreeIndeces.Enqueue(i);
                }
            }
        }

        private static World AddWorld(int index)
        {
            lock (Instance.worlds)
            {
                var newWorld = new World(index);
                Instance.worlds[index] = newWorld;
                OnNewWorldAdded?.Invoke(newWorld);
                return newWorld;
            }
        }

        public static bool TryGetEntityByComponents<T>(out Entity entity, World world = null) where T : IComponent, new()
        {
            if (world == null)
                world = Default;

            var components = ComponentProvider<T>.ComponentsToWorld.Data[world.Index].Components;

            for (int i = 0; i < components.Length; i++)
            {
                T c = components[i];
                if (c != null && c.IsAlive)
                {
                    entity = world.Entities[i];
                    return true;
                }
            }

            entity = null;
            return false;
        }

        public static void RemoveWorld(int index, bool dispose = true)
        {
            lock (Instance.worlds)
            {
                var needWorld = Worlds[index];
                Worlds[index] = null;

                if (dispose)
                    needWorld.Dispose();

                WorldRemoved?.Invoke(needWorld);
                worldsFreeIndeces.Enqueue(index);
            }
        }

        public static void RemoveWorld(World world)
        {
            RemoveWorld(world.Index);
        }

        /// <summary>
        /// Этот метод цепляется к ивенту закрытия приложения и рассылает его по всем системам, размеченным интерфейсом <code>IOnApplicationQuit</code>
        /// </summary>
        public static void OnApplicationExitInvoke()
        {
            foreach (var world in Worlds)
                foreach (var entity in world.Entities)
                    foreach (ISystem system in entity.Systems)
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
        public static void Command<T>(T command, int world = 0) where T : struct, IGlobalCommand
        {
            if (world == -1)
            {
                var count = Worlds.Length;

                for (int i = 0; i < count; i++)
                    Worlds[i]?.Command(command);
                return;
            }

            if (world < Worlds.Length)
                Instance.worlds[world].Command(command);
        }

        public static void RegisterEntity(Entity entity, bool add)
        {
            entity.World.RegisterEntity(entity, add);
        }

        public static bool TryGetEntityByComponent<T>(out Entity outEntity, int worldIndex = 0) where T : IComponent, new()
        {
            if (worldIndex == -1)
            {
                foreach (var w in Worlds)
                {
                    if (w == null)
                        continue;

                    if (w.TryGetEntityByComponent<T>(out outEntity))
                    {
                        return true;
                    }
                }

                outEntity = null;
                return false;
            }

            var world = Instance.worlds[worldIndex];
            return world.TryGetEntityByComponent<T>(out outEntity);
        }

        /// <summary>
        /// на самом деле возвращаем первый попавшийся/закешированный, то что он единственный и неповторимый - на вашей совести
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="worldIndex"></param>
        /// <returns></returns>
        public static T GetSingleSystem<T>(int worldIndex = 0) where T : ISystem => Instance.worlds[worldIndex].GetSingleSystem<T>();

        /// <summary>
        /// in fact, we return the first one that came across / cached, the fact that it is the one and only - on your conscience
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="worldIndex"></param>
        /// <returns></returns>
        public static T GetSingleComponent<T>(int worldIndex = 0) where T : IComponent, IWorldSingleComponent, new() => Instance.worlds[worldIndex].GetSingleComponent<T>();

        public static bool TryGetSingleComponent<T>(int world, out T component) where T : IComponent, IWorldSingleComponent => Instance.worlds[world].TryGetSingleComponent<T>(out component);

        public static bool TryGetWorld<T>(out World world) where T : IComponent, IWorldSingleComponent
        {
            foreach (var w in Worlds)
            {
                if (w.TryGetSingleComponent<T>(out _))
                {
                    world = w;
                    return true;
                }
            }

            world = null;
            return false;
        }

        public static bool TryGetWorldAndComponent<T>(out World world, out T component) where T : IComponent, IWorldSingleComponent
        {
            foreach (var w in Worlds)
            {
                if (w.TryGetSingleComponent<T>(out component))
                {
                    world = w;
                    return true;
                }
            }

            component = default;
            world = null;
            return false;
        }

        public static bool TryGetEntityByID(Guid entityGuid, out Entity entity, int worldIndex = 0)
        {
            foreach (var w in Worlds)
            {
                if (w.TryGetEntityByID(entityGuid, out entity))
                    return true;
            }

            entity = default;
            return false;
        }

        public void Dispose()
        {
            for (int i = 0; i < worlds.Length; i++)
            {
                worlds[i]?.Dispose();
            }

            Array.Clear(worlds, 0, worlds.Length);
            worldsFreeIndeces.Clear();
            Instance = null;
        }

        public static void RecreateInstance()
        {
            Instance?.Dispose();
            new EntityManager();
        }
    }
}