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
        public const string ClearEntity = "CEntity";

        public readonly HECSList<ISystem> Systems = new HECSList<ISystem>();
        public readonly HashSet<int> Components = new HashSet<int>(8);

        public int WorldId => World.Index;
        public World World;

        public Guid GUID;
        public string ID = ClearEntity;

        public bool IsRegistered;
        public bool IsInited;
        public bool IsAlive = true;
        public bool IsPaused;
        public bool IsDisposed;

        public int Index;
        public bool IsDirty;
        public int Generation;

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

        public static Entity Get(string id)
        {
            var entity = EntityManager.Default.GetEntityFromPool(id);
            entity.GenerateGuid();
            return entity;
        }

        public static Entity Get(World world, string id)
        {
            var entity = world.GetEntityFromPool(id);
            entity.GenerateGuid();
            return entity;
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
            if (IsInited)
                return;

            World.RegisterEntity(this, true);
        }

        public void Command<T>(T command) where T : struct, ICommand
        {
            if (IsPaused || !IsAlive)
                return;

            LocalCommandListener<T>.Invoke(World.Index, Index, command);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            foreach (var s in Systems)
                s.BeforeDispose();

            foreach (var c in Components)
                this.GetComponent(c).BeforeDispose();

            IsDisposed = true;
            Clean();
            World.RegisterEntity(this, false);
        }

        private void Clean()
        {
            var poolComponents = HECSPooledArray<int>.GetArray(Components.Count);

            foreach (var c in Components)
                poolComponents.Add(c);

            for (int i = 0; i < poolComponents.Count; i++)
                World.GetComponentProvider(poolComponents.Items[i]).RemoveComponent(Index);

            var pool = HECSPooledArray<ISystem>.GetArray(Systems.Count);

            foreach (var s in Systems)
                pool.Add(s);

            for (int i = 0; i < pool.Count; i++)
                RemoveHecsSystem(pool.Items[i]);

            Systems.Clear();
            Components.Clear();

            pool.Release();
            poolComponents.Release();
        }

        public T GetSystem<T>() where T : ISystem
        {
            foreach (var s in Systems)
                if (s is T needed)
                    return needed;

            return default;
        }

        public T GetSystemTypeOf<T>()
        {
            foreach (var s in Systems)
                if (s is T needed)
                    return needed;

            return default;
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
        {
            UnityPart();
            Dispose();
        }

        partial void UnityPart();

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

        public HECSPooledArray<T> GetComponentsOfTypePooled<T>()
        {
            var pool = HECSPooledArray<T>.GetArray(Components.Count);

            foreach (var c in Components)
            {
                if (World.GetComponentProvider(c).GetIComponent(Index) is T needed)
                    pool.Add(needed);
            }

            return pool;
        }

        public T GetComponentTypeOf<T>() where T : class
        {
            foreach (var c in Components)
            {
                if (World.GetComponentProvider(c).GetIComponent(Index) is T needed)
                    return needed;
            }

            return default;
        }

        /// <summary>
        /// this is for editor only, do not use it runtime logic
        /// </summary>
        /// <returns></returns>
        public List<IComponent> DebugGetListOfComponents()
        {
            var list = new List<IComponent>();

            foreach (var c in Components)
            {
                list.Add(World.GetComponentProvider(c).GetIComponent(Index));
            }

            return list;
        }

        public override int GetHashCode()
        {
            return -762187988 + GUID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Entity entity && entity.ID == ID;
        }

        #region Mask

        public bool ContainsMask<T>() where T : IComponent, new()
        {
            var index = ComponentProvider<T>.TypeIndex;
            return Components.Contains(index);
        }

        public bool ContainsMask(Filter mask)
        {
            for (int i = 0; i < mask.Lenght; i++)
            {
                if (!Components.Contains(mask[i]))
                    return false;
            }

            return true;
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

        public bool ContainsAnyFromMask(Filter mask)
        {
            for (int i = 0; i < mask.Lenght; i++)
            {
                if (Components.Contains(mask[i]))
                    return true;
            }

            return false;
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
                World.AdditionalProcessing(system, this, true);
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
            if (!system.IsDisposed)
                system.Dispose();

            World.UnRegisterSystem(system);

            //system.ReturnToPool();
            return Systems.RemoveSwap(system);
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
            {
                Dispose();
                IsInited = false;
            }

            foreach (var c in components)
                this.AddComponent(c);

            foreach (var s in systems)
                AddHecsSystem(s);

            Init();
        }
    }

    public readonly struct AliveEntity
    {
        public readonly Entity Entity;
        public readonly int Generation;

        public AliveEntity(Entity entity)
        {
            Entity = entity;
            Generation = entity.IsAlive() ? entity.Generation : 0;
        }

        public bool IsAlive => Entity.IsAlive(Generation);
    }
}