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
        private IRegisterService RegisterService = new RegisterService();
        private readonly List<ISystem> systems = new List<ISystem>();
        private readonly HashSet<int> components = new HashSet<int>(8);

        public int WorldId => World.Index;

        public World World { get; private set; }

        public Guid GUID { get; private set; }
        public string ID { get; private set; }

        private IAddSingleComponent addSingleComponent => World;

        public HashSet<int> Components => components; 
        public List<ISystem> GetAllSystems => systems;

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
                var container = this.GetHECSComponent<ActorContainerID>();

                if (container != null)
                    return container.ID;

                return "Container Empty";
            }
        }
        public int EntityIndex => entityIndex;

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

        /// <summary>
        /// Base method for add hecs component, all methods with adds component functionality shoulds use this method at the end
        /// </summary>
        /// <param name="component"></param>
        /// <param name="owner">this for actor, actor can assign self as owner</param>
        /// <param name="silently"></param>
        public T AddHecsComponent<T>(T component, IEntity owner = null, bool silently = false) where T : IComponent
        {
            if (component == null)
                throw new Exception($"compontent is null " + ID);

            component.ComponentsMask = TypesMap.GetComponentInfo(component).ComponentsMask;

            if (components[component.ComponentsMask.Index] != null)
            {
                HECSDebug.LogWarning("we add copy of component " + component.GetTypeHashCode);
                return (T)components[component.ComponentsMask.Index];
            }

            if (owner == null)
                component.Owner = this;
            else
            {
                component.Owner = owner;
                ComponentAdditionalProcessing(component, owner);
            }

            components[component.ComponentsMask.Index] = component;
            TypesMap.SetComponent(this, component);
            component.IsAlive = true;

            if (IsInited && !component.IsRegistered)
            {
                if (component is IInitable initable)
                    initable.Init();

                if (component is IAfterEntityInit afterEntityInit)
                    afterEntityInit.AfterEntityInit();

                if (component is IWorldSingleComponent worldSingleComponent)
                    addSingleComponent.AddSingleWorldComponent(worldSingleComponent, true);
            }

            if (!silently && IsInited && !component.IsRegistered)
            {
                World.AddOrRemoveComponent(component, true);
                TypesMap.RegisterComponent(component.ComponentsMask.Index, component.Owner, true);
                component.SetIsRegistered();
            }

            return component;
        }

        public bool TryGetHecsComponent<T>(HECSMask mask, out T component) where T : IComponent
        {
            var needed = components[mask.Index];

            if (needed != null && needed is T cast)
            {
                component = cast;
                return true;
            }

            component = default;
            return false;
        }

        public void GetHecsComponents<T>(ref List<T> outComponents) where T : IComponent
        {
            outComponents.Clear();
            foreach (var c in components)
            {
                if (c != null && c is T needed)
                    outComponents.Add(needed);
            }
        }

        public void Init(int worldIndex, bool needRegister = true)
        {
            World = EntityManager.Worlds.Data[worldIndex];
            Init(needRegister);
        }

        public void Init(bool needRegister = true)
        {
            if (IsInited)
                throw new InvalidOperationException($"Character already inited: {GUID}");

            if (World == null)
                World = EntityManager.Default;

            InitComponentsAndSystems();

            if (needRegister)
                EntityManager.RegisterEntity(this, true);

            AfterInit();

            for (int i = 0; i < ComponentsMask.CurrentIndexes.Count; i++)
            {
                var c = components[ComponentsMask.CurrentIndexes[i]];

                if (c.IsRegistered)
                    continue;

                RegiserComponent(c);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RegiserComponent(IComponent component)
        {
            World?.AddOrRemoveComponent(component, true);
            TypesMap.RegisterComponent(component.ComponentsMask.Index, component.Owner, true);
            component.SetIsRegistered();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AfterInit()
        {
            foreach (var c in components)
                AfterInitComponent(c);

            foreach (var s in systems)
                AfterInitSystem(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AfterInitComponent(IComponent component)
        {
            if (component != null)
                if (component is IAfterEntityInit afterEntityInit)
                    afterEntityInit.AfterEntityInit();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AfterInitSystem(ISystem system)
        {
            if (system is IAfterEntityInit afterSysEntityInit)
                afterSysEntityInit.AfterEntityInit();
        }

        public void InitComponentsAndSystems()
        {
            InitComponents(components);
            InitSystems(systems);
            IsInited = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitSystems(List<ISystem> systems)
        {
            foreach (var sys in systems)
            {
                if (sys == null)
                {
                    HECSDebug.LogError("we have null system " + ID);
                    continue;
                }

                TypesMap.BindSystem(sys);
                sys.InitSystem();
                RegisterService.RegisterSystem(sys);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitComponents(IComponent[] components)
        {
            foreach (var component in components)
            {
                if (component == null)
                    continue;


                InitComponent(component);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitComponents(List<IComponent> components)
        {
            foreach (var component in components)
            {
                if (component == null)
                    continue;


                InitComponent(component);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitComponent(IComponent component)
        {
            if (component is IInitable init)
                init.Init();

            if (component is IWorldSingleComponent worldSingleComponent)
                addSingleComponent.AddSingleWorldComponent(worldSingleComponent, true);
        }

        public void Command<T>(T command) where T : struct, ICommand
        {
            if (IsPaused || !IsAlive)
                return;

            EntityCommandService.Invoke(command);
        }

        public void RemoveHecsComponent(IComponent component)
        {
            if (component == null)
                return;

            if (component is IWorldSingleComponent worldSingleComponent)
                addSingleComponent.AddSingleWorldComponent(worldSingleComponent, false);

            if (component is IDisposable disposable)
                disposable.Dispose();

            if (IsInited)
                TypesMap.RegisterComponent(component.ComponentsMask.Index, component.Owner, false);

            components[component.ComponentsMask.Index] = null;
            TypesMap.RemoveComponent(this, component);
            ComponentsMask.RemoveMask(component.ComponentsMask.Index);

            component.IsAlive = false;

            if (IsInited)
                World?.AddOrRemoveComponent(component, false);
        }

        private void Reset()
        {
            GenerateId();
        }

        public void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem
        {
            if (owner == null)
                system.Owner = this;
            else
            {
                system.Owner = owner;
                SystemAdditionalProcessing(system, owner);
            }

            foreach (var s in systems)
            {
                if (s.GetTypeHashCode == system.GetTypeHashCode)
                    return;
            }

            systems.Add(system);

            if (IsInited)
            {
                TypesMap.BindSystem(system);
                system.InitSystem();
                RegisterService.RegisterSystem(system);

                if (system is IAfterEntityInit afterSysEntityInit)
                    afterSysEntityInit.AfterEntityInit();
            }
        }

        //this method for actor
        public void SetIsInited()
        {
            IsInited = true;
        }

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

        public void Dispose()
        {
            if (!EntityManager.IsAlive || World == null || !World.IsAlive)
                return;

            EntityManager.RegisterEntity(this, false);
            IsAlive = false;
            IsPaused = true;

            foreach (var s in systems)
            {
                if (s.IsDisposed)
                    continue;

                s.Dispose();
            }

            foreach (var s in systems.ToArray())
                RemoveHecsSystem(s);



            for (int i = 0; i < components.Length; i++)
            {
                IComponent c = components[i];

                if (c != null)
                    RemoveHecsComponent(c);
            }

            systems.Clear();
            Array.Clear(components, 0, components.Length);

            EntityCommandService.Dispose();
            RegisterComponentListenersService.Dispose();
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

        public void RemoveHecsSystem(ISystem system)
        {
            if (IsInited)
                RegisterService.UnRegisterSystem(system);

            if (!system.IsDisposed)
                system.Dispose();

            systems.Remove(system);
        }

        public bool Equals(IEntity other)
        {
            return other.GUID == GUID;
        }

        public T GetOrAddComponent<T>(IEntity owner = null) where T : class, IComponent
        {
            var index = TypesMap.GetIndexByType<T>();
            var needed = components[index];

            if (needed != null)
                return (T)needed;

            var newComp = TypesMap.GetComponentFromFactory<T>();
            AddHecsComponent(newComp, owner);
            return newComp;
        }

        public void RemoveHecsComponent(HECSMask component)
        {
            var needed = components[component.Index];
            RemoveHecsComponent(needed);
        }

        public void InjectEntity(IEntity entity, IEntity owner = null, bool additive = false)
        {
            if (!additive)
                RemoveComponentsAndSystems();

            foreach (var c in entity.GetAllComponents)
            {
                if (c != null)
                    AddHecsComponent(c, owner);
            }

            foreach (var s in entity.GetAllSystems)
                AddHecsSystem(s, owner);

            GUID = entity.GUID;
        }

        private void RemoveComponentsAndSystems()
        {
            for (int i = 0; i < components.Length; i++)
                RemoveHecsComponent(components[i]);

            foreach (var s in systems.ToArray())
                RemoveHecsSystem(s);
        }

        public void GenerateGuid()
        {
            GUID = Guid.NewGuid();
        }

        public bool ContainsMask(ref HECSMask mask)
        {
            return GetAllComponents[mask.Index] != null;
        }

        public bool ContainsAnyFromMask(FilterMask mask)
        {
            return ComponentsMask.ContainsAny(mask);
        }

        public bool ContainsAnyFromMask(HECSMultiMask mask)
        {
            return ComponentsMask.ContainsAny(mask);
        }

        public bool ContainsMask(HECSMultiMask mask)
        {
            return ComponentsMask.Contains(mask);
        }

        public void HecsDestroy()
            => Dispose();

        public void SetGuid(Guid guid)
        {
            GUID = guid;
        }

        public void AddOrReplaceComponent(IComponent component, IEntity owner = null, bool silently = false)
        {
            var index = TypesMap.GetComponentInfo(component);
            if (GetAllComponents[index.ComponentsMask.Index] != null)
                RemoveHecsComponent(index.ComponentsMask);

            AddHecsComponent(component, owner, silently);
        }

        partial void ComponentAdditionalProcessing(IComponent component, IEntity owner);
        partial void SystemAdditionalProcessing(ISystem system, IEntity owner);

        public bool ContainsMask<T>() where T : IComponent
        {
            var index = TypesMap.GetIndexByType<T>();
            return components[index] != null;
        }

        public void RemoveHecsComponent<T>() where T : IComponent
        {
            var index = TypesMap.GetIndexByType<T>();
            if (components[index] != null)
                RemoveHecsComponent(components[index]);
        }

        public bool TryGetHecsComponent<T>(out T component) where T : IComponent
        {
            var index = TypesMap.GetIndexByType<T>();

            if (components[index] != null)
            {
                component = (T)components[index];
                return true;
            }

            component = default;
            return false;
        }

        public IEnumerable<T> GetComponentsByType<T>()
        {
            for (int i = 0; i < ComponentsMask.CurrentIndexes.Count; i++)
            {
                if (components[ComponentsMask.CurrentIndexes[i]] is T needed)
                    yield return needed;
            }
        }

        public bool ContainsMask(FilterMask mask)
        {
            return ComponentsMask.Contains(mask);
        }

        public override int GetHashCode()
        {
            return -762187988 + GUID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is IEntity entity && entity.GUID == GUID;
        }

        public bool RemoveHecsSystem<T>() where T : ISystem
        {
            foreach (var s in systems)
            {
                if (s is T)
                {
                    RemoveHecsSystem(s);
                    return true;
                }
            }

            return false;
        }



        /// <summary>
        /// we can replace or add data|logic on this entity, if for cases when we need totaly replace behaviour on entity
        /// </summary>
        /// <param name="components"></param>
        /// <param name="systems"></param>
        /// <param name="isAdditive"></param>
        public void Inject(List<IComponent> components, List<ISystem> systems, bool isAdditive = false, IEntity owner = null)
        {
            if (!isAdditive)
                RemoveComponentsAndSystems();

            IsInited = false;

            foreach (var c in components)
                AddHecsComponent(c, owner);

            foreach (var s in systems)
                AddHecsSystem(s, owner);

            InitComponents(components);
            InitSystems(systems);

            foreach (var c in components)
                AfterInitComponent(c);

            foreach (var s in systems)
                AfterInitSystem(s);

            IsInited = true;

            foreach (var c in components)
                RegiserComponent(c);
        }



        public bool ContainsMask(HashSet<int> mask)
        {
            throw new NotImplementedException();
        }

        public bool ContainsAnyFromMask(HashSet<int> mask)
        {
            throw new NotImplementedException();
        }

        public void Init(World world)
        {
            throw new NotImplementedException();
        }

        public void SetID(int index)
        {
            throw new NotImplementedException();
        }

        public bool ContainsMask(int mask)
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