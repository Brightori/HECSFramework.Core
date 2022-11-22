using System;

namespace HECSFramework.Core
{
    public partial class EntityManager : IDisposable
    {
        public const int AllWorld = -1;

        private ConcurrencyList<World> worlds;
        private static EntityManager Instance;

        public static ConcurrencyList<World> Worlds => Instance.worlds;
        public static World Default => Instance.worlds.Data[0];
        public static bool IsAlive => Instance != null;

        public static event Action<World> OnNewWorldAdded;


        public EntityManager(int worldsCount = 1)
        {
            worlds = new ConcurrencyList<World>(worldsCount);
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

        /// <summary>
        /// Если нам нужно убедиться что такая ентити существует, или дождаться когда она появиться, 
        /// то мы отправляем команду ожидать появления нужной сущности
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="waitForComponent"></param>
        public static void Command<T>(T command, ref HECSMask waitForComponent, int worldIndex = 0) where T : struct, ICommand, IGlobalCommand
            => Worlds.Data[worldIndex].Command(command, ref waitForComponent);

        public static void RegisterEntity(IEntity entity, bool add)
        {
            entity.World.RegisterEntity(entity, add);
        }

        public static ConcurrencyList<IEntity> Filter(FilterMask include, int worldIndex = 0) => Instance.worlds.Data[worldIndex].Filter(include);
        public static ConcurrencyList<IEntity> Filter(FilterMask include, FilterMask exclude, int worldIndex = 0) => Instance.worlds.Data[worldIndex].Filter(include, exclude);
        public static ConcurrencyList<IEntity> Filter(HECSMask mask, int worldIndex = 0) => Instance.worlds.Data[worldIndex].Filter(new FilterMask(mask));

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

            var world = Instance.worlds.Data[worldIndex];
            return world.TryGetEntityByComponents(out outEntity, ref mask);
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
        public static T GetSingleComponent<T>(int worldIndex = 0) where T : IComponent, IWorldSingleComponent => Instance.worlds.Data[worldIndex].GetSingleComponent<T>();
        public static T GetSingleComponent<T>(HECSMask mask, int worldIndex = 0) where T : IComponent, IWorldSingleComponent => Instance.worlds.Data[worldIndex].GetSingleComponent<T>(mask);

        public static bool TryGetSingleComponent<T>(int world, out T component) where T : IComponent, IWorldSingleComponent => Worlds.Data[world].TryGetSingleComponent<T>(out component);
        public static bool TryGetSingleComponent<T>(int world, HECSMask mask, out T component) where T : IComponent, IWorldSingleComponent => Worlds.Data[world].TryGetSingleComponent<T>(mask, out component);

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

        public bool TryGetSystemFromEntity<T>(ref HECSMask mask, out T system, int worldIndex = 0) where T : ISystem
        {
            var world = Instance.worlds.Data[worldIndex];
            return world.TryGetSystemFromEntity(ref mask, out system);
        }

        public static void AddOrRemoveComponent(IComponent component, bool isAdded)
        {
            Instance.worlds.Data[component.Owner.WorldId].AddOrRemoveComponent(component, isAdded);
        }

        public bool TryGetComponentFromEntity<T>(out T component, ref HECSMask owner, ref HECSMask neededComponent, int worldIndex) where T : IComponent
            => worlds.Data[worldIndex].TryGetComponentFromEntity(out component, ref owner, ref neededComponent);

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