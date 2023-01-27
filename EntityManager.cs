using System;
using TMPro;

namespace HECSFramework.Core
{
    public partial class EntityManager : IDisposable
    {
        public const int AllWorld = -1;

        private HECSList<World> worlds;
        private static EntityManager Instance;

        public static HECSList<World> Worlds => Instance.worlds;
        public static World Default => Instance.worlds.Data[0];
        public static bool IsAlive => Instance != null;

        public static event Action<World> OnNewWorldAdded;


        public EntityManager(int worldsCount = 1)
        {
            worlds = new HECSList<World>(worldsCount);
            Instance = this;

            for (int i = 0; i < worldsCount; i++)
            {
                AddWorld();
            }

            foreach (var world in worlds)
            {
                world.Init();
            }
        }

        public static World AddWorld()
        {
            lock (Instance.worlds)
            {
                var newWorld = new World(Worlds.Count);
                Instance.worlds.Add(newWorld);
                OnNewWorldAdded?.Invoke(newWorld);
                return newWorld;
            }
        }

        public static bool TryGetEntityByComponents<T>(out Entity entity, World world = null) where T: IComponent, new()
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
                var needWorld = Worlds.Data[index];
                Worlds.RemoveAt(index);

                if (dispose)
                    needWorld.Dispose();

                for (int i = 0; i < Worlds.Count; i++)
                {
                    Worlds.Data[i].UpdateIndex(i);
                }
            }
        }

        public static void RemoveWorld(World world)
        {
            var index = Worlds.IndexOf(world);
            RemoveWorld(index);
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
                var count = Worlds.Count;

                for (int i = 0; i < count; i++)
                    Worlds.Data[i].Command(command);
                return;
            }

            Instance.worlds.Data[world].Command(command);
        }
       
        public static void RegisterEntity(Entity entity, bool add)
        {
            entity.World.RegisterEntity(entity, add);
        }

        //todo filter
        //public static HECSList<Entity> Filter(FilterMask include, int worldIndex = 0) => Instance.worlds.Data[worldIndex].Filter(include);
        //public static HECSList<Entity> Filter(FilterMask include, FilterMask exclude, int worldIndex = 0) => Instance.worlds.Data[worldIndex].Filter(include, exclude);
        //public static HECSList<Entity> Filter(HECSMask mask, int worldIndex = 0) => Instance.worlds.Data[worldIndex].Filter(new FilterMask(mask));

       
        public static bool TryGetEntityByComponent<T>(out Entity outEntity, int worldIndex = 0) where T : IComponent, new()
        {
            if (worldIndex == -1)
            {
                foreach (var w in Worlds)
                {
                    if (w.TryGetEntityByComponent<T>(out outEntity))
                    {
                        return true;
                    }
                }

                outEntity = null;
                return false;
            }

            var world = Instance.worlds.Data[worldIndex];
            return world.TryGetEntityByComponent<T>(out outEntity);
        }

        /// <summary>
        /// на самом деле возвращаем первый попавшийся/закешированный, то что он единственный и неповторимый - на вашей совести
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="worldIndex"></param>
        /// <returns></returns>
        public static T GetSingleSystem<T>(int worldIndex = 0) where T : ISystem => Instance.worlds.Data[worldIndex].GetSingleSystem<T>();

        /// <summary>
        /// in fact, we return the first one that came across / cached, the fact that it is the one and only - on your conscience
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="worldIndex"></param>
        /// <returns></returns>
        public static T GetSingleComponent<T>(int worldIndex = 0) where T : IComponent, IWorldSingleComponent, new() => Instance.worlds.Data[worldIndex].GetSingleComponent<T>();

        public static bool TryGetSingleComponent<T>(int world, out T component) where T : IComponent, IWorldSingleComponent => Worlds.Data[world].TryGetSingleComponent<T>(out component);

        public static bool TryGetWorld<T>(out World world) where T: IComponent, IWorldSingleComponent
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

        public static void AddOrRemoveComponent(IComponent component, bool isAdded)
        {
            Instance.worlds.Data[component.Owner.WorldId].AddOrRemoveComponent(component, isAdded);
        }

        public void Dispose()
        {
            for (int i = 0; i < worlds.Count; i++)
            {
                worlds.Data[i].Dispose();
            }

            worlds.Clear();
            Instance = null;
        }

        public static void RecreateInstance()
        {
            Instance?.Dispose();
            new EntityManager();
        }
    }
}