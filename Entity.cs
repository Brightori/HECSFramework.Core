using Components;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    [Serializable]
    public sealed partial class Entity : IEntity
    {
        private int entityIndex;
        private readonly HECSList<ISystem> systems = new HECSList<ISystem>();
        private readonly HashSet<int> components = new HashSet<int>(8);

        public int WorldId => World.Index;

        public World World { get; private set; }

        public Guid GUID { get; private set; }
        public string ID { get; private set; }

        public HashSet<int> Components => components; 
        public HECSList<ISystem> GetAllSystems => systems;

        public EntityLocalCommandService EntityCommandService { get; } = new EntityLocalCommandService();
        public LocalComponentListenersService RegisterComponentListenersService { get; } = new LocalComponentListenersService();

        public bool IsInited { get; private set; }
        public bool IsAlive { get; private set; } = true;
        public bool IsPaused { get; private set; }

        /// <summary>
        /// this is slow method, purpose - using at Editor or for debugging
        /// better will take ActorContainerID directly - GetActorContainerID
        /// </summary>
        public string ContainerID
        {
            get
            {
                var container = this.GetComponent<ActorContainerID>();

                if (container != null)
                    return container.ID;

                return "Container Empty";
            }
        }
        public int Index => entityIndex;

        public bool IsDirty { get; }
        public int Generation { get; set; }

        public Entity(string id = "Empty")
        {
            World = EntityManager.Default;
            entityIndex = EntityManager.Default.GetEntityFreeIndex();
            GenerateGuid();
        }

        public Entity(World world, string id = "Empty")
        {
            GenerateGuid();
            entityIndex = world.GetEntityFreeIndex();
        }

        /// <summary>
        /// this constructor by default used by world for making entities, 
        /// here u should provide free index from world
        /// </summary>
        /// <param name="world">u should provide world here</param>
        /// <param name="id">this is id or name of entity</param>
        /// <param name="index"></param>
        public Entity(World world, int index, string id = "Empty")
        {
            GenerateGuid();
            entityIndex = index;
        }

        public void SetID(string id)
        {
            ID = id;
        }

        public void Init(World world)
        {
           
        }



        public void Command<T>(T command) where T : struct, ICommand
        {
            if (IsPaused || !IsAlive)
                return;

            EntityCommandService.Invoke(command);
        }

        //this method for actor
       
      

        public void Dispose()
        {
           
        }

        public bool TryGetSystem<T>(out T system) where T : ISystem
        {
            foreach (var s in systems)
            {
                if (s is T casted)
                {
                    system = casted;
                    return true;
                }
            }

            system = default;
            return false;
        }

        public void GenerateId()
        {
            GUID = System.Guid.NewGuid();
        }

        public bool Equals(IEntity other)
        {
            return other.GUID == GUID;
        }

        public void GenerateGuid()
        {
            GUID = Guid.NewGuid();
        }

        public void HecsDestroy()
            => Dispose();

        public void SetGuid(Guid guid)
        {
            GUID = guid;
        }

        public IEnumerable<T> GetComponentsByType<T>() 
        {
            foreach (var c in components)
            {
                if (World.GetComponentProvider(c).GetIComponent(Index) is T needed)
                    yield return needed;
            }
        }

        public void SetID(int index)
        {
            entityIndex = index;
        }


        public override int GetHashCode()
        {
            return -762187988 + GUID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is IEntity entity && entity.GUID == GUID;
        }

     


        #region Mask

        public bool ContainsMask<T>() where T : IComponent, new()
        {
            var index = ComponentProvider<T>.TypeIndex;
            return components.Contains(index);
        }

        public bool ContainsMask(HashSet<int> mask)
        {
            foreach (var m in mask)
            {
                foreach (var c in components)
                {
                    if (m != c)
                        return false;
                }
            }

            return true;
        }

        public bool ContainsAnyFromMask(HashSet<int> mask)
        {
            foreach (var m in mask)
            {
                foreach (var c in components)
                {
                    if (m == c)
                        return true;
                }
            }

            return false;
        }
       
      
        public bool ContainsMask(int mask)
        {
            return components.Contains(mask);
        }
        #endregion

        #region Pause
        public void Pause()
        {
            IsPaused = true;

            foreach (var sys in systems)
            {
                if (sys is IHavePause havePause)
                    havePause.Pause();
            }
        }

        public void UnPause()
        {
            IsPaused = false;

            foreach (var sys in systems)
            {
                if (sys is IHavePause havePause)
                    havePause.UnPause();
            }
        }
        #endregion

        public void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem
        {
            throw new NotImplementedException();
        }

        public void RemoveHecsSystem(ISystem system)
        {
            throw new NotImplementedException();
        }

        public bool RemoveHecsSystem<T>() where T : ISystem
        {
            throw new NotImplementedException();
        }

        public void Inject(List<IComponent> components, List<ISystem> systems, bool isAdditive = false, IEntity owner = null)
        {
            throw new NotImplementedException();
        }
    }

    public interface IChangeWorld
    {
        void SetWorld(World world);
        void SetWorld(int world);
    }
}