using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using Helpers;

namespace HECSFramework.Core
{
    [Serializable]
    public sealed partial class Entity
    {
        public readonly HECSList<ISystem> Systems = new HECSList<ISystem>();
        public readonly HashSet<int> Components = new HashSet<int>(8);

        public int WorldId => World.Index;
        public World World;

        public Guid GUID;
        public string ID;

        public readonly EntityLocalCommandService EntityCommandService = new EntityLocalCommandService();

        public bool IsInited;
        public bool IsAlive = true;
        public bool IsPaused;

        public int Index;
        public bool IsDirty;

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

        public Entity(string id = "Empty")
        {
            World = EntityManager.Default;
            Index = EntityManager.Default.GetEntityFreeIndex();
            World.Entities[Index] = this;
            GenerateGuid();
        }

        public Entity(World world, string id = "Empty")
        {
            World = world;
            Index = world.GetEntityFreeIndex();
            World.Entities[Index] = this;
            GenerateGuid();
        }

        /// <summary>
        /// this constructor for worlds, they fill entitis pooling through this
        /// </summary>
        /// <param name="index"></param>
        /// <param name="world"></param>
        /// <param name="id"></param>
        public Entity(int index, World world, string id = "Empty")
        {
            GenerateGuid();
            World = world;
            Index = index;
            ID = id;
        }

        public void SetID(string id)
        {
            ID = id;
        }

        public void Init()
        {
            World.RegisterEntity(this, true);
        }

        public void Command<T>(T command) where T : struct, ICommand
        {
            if (IsPaused || !IsAlive)
                return;

            EntityCommandService.Invoke(command);
        }


        public void Dispose()
        {
            Clean();
            World.RegisterEntity(this, false);
        }

        private void Clean()
        {
            foreach (var c in Components)
                World.GetComponentProvider(c).RemoveComponent(Index);

            var pool = HECSPooledArray<ISystem>.GetArray(Systems.Count);

            foreach (var s in Systems)
                pool.Add(s);

            for (int i = 0; i < pool.Count; i++)
                RemoveHecsSystem(pool.Items[i]);

            Systems.Clear();
            Components.Clear();
            pool.Release();
        }

        public bool TryGetSystem<T>(out T system) where T : ISystem
        {
            foreach (var s in Systems)
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

        public bool Equals(Entity other)
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
            foreach (var c in Components)
            {
                if (World.GetComponentProvider(c).GetIComponent(Index) is T needed)
                    yield return needed;
            }
        }

        public override int GetHashCode()
        {
            return -762187988 + GUID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Entity entity && entity.GUID == GUID;
        }

        #region Mask

        public bool ContainsMask<T>() where T : IComponent, new()
        {
            var index = ComponentProvider<T>.TypeIndex;
            return Components.Contains(index);
        }

        public bool ContainsMask(HashSet<int> mask)
        {
            foreach (var m in mask)
            {
                foreach (var c in Components)
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
                foreach (var c in Components)
                {
                    if (m == c)
                        return true;
                }
            }

            return false;
        }


        public bool ContainsMask(int mask)
        {
            return Components.Contains(mask);
        }
        #endregion

        #region Pause
        public void Pause()
        {
            IsPaused = true;

            foreach (var sys in Systems)
            {
                if (sys is IHavePause havePause)
                    havePause.Pause();
            }
        }

        public void UnPause()
        {
            IsPaused = false;

            foreach (var sys in Systems)
            {
                if (sys is IHavePause havePause)
                    havePause.UnPause();
            }
        }
        #endregion

        public bool AddHecsSystem<T>(T system) where T : ISystem
        {
            system.Owner = this;

            foreach (var s in Systems)
            {
                if (s.GetTypeHashCode == system.GetTypeHashCode)
                    return false;
            }

            if (IsInited)
            {
                TypesMap.BindSystem(system);
                system.InitSystem();
                World.RegisterSystem(system);

                if (system is IAfterEntityInit afterSysEntityInit)
                    afterSysEntityInit.AfterEntityInit();
            }

            Systems.Add(system);
            return true;
        }

        public bool RemoveHecsSystem(ISystem system)
        {
            if (IsInited)
                World.UnRegisterSystem(system);

            if (!system.IsDisposed)
                system.Dispose();

            system.ReturnToPool();
            return Systems.Remove(system);
        }

        public bool RemoveHecsSystem<T>() where T : ISystem
        {
            var needed = (T)Systems.FirstOrDefault(x => x is T);

            if (needed != null)
                return RemoveHecsSystem(needed);

            return false;
        }

        public void Inject(List<IComponent> components, List<ISystem> systems, bool isAdditive = false, Entity owner = null)
        {
            if (!isAdditive)
                Dispose();


        }
    }
}