using Components;
using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    [Serializable]
    public sealed partial class Entity : IEntity, IChangeWorld
    {
        public int WorldId => World.Index;

        public World World { get; private set; }

        public Guid GUID { get; private set; }
        public ComponentContext ComponentContext { get; } = new ComponentContext();
        public string ID { get; private set; }

        public Entity(string id)
        {
            GenerateId();
            ID = id;
        }

        private List<ISystem> systems = new List<ISystem>();
        public List<ISystem> GetAllSystems => systems;

        private IComponent[] components = new IComponent[TypesMap.SizeOfComponents];
        public IComponent[] GetAllComponents => components;
        private IRegisterService RegisterService = new RegisterService();
        public EntityLocalCommandService EntityCommandService { get; } = new EntityLocalCommandService();

        public bool IsInited { get; private set; }
        public bool IsAlive { get; private set; } = true;
        public bool IsPaused { get; private set; }
        public bool IsLoaded { get; set; }

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

        public HECSMultiMask ComponentsMask { get; } = new HECSMultiMask();
        public LocalComponentListenersService RegisterComponentListenersService { get; } = new LocalComponentListenersService();

        public Entity() { }

        public Entity(string id, int worldIndex)
        {
            ID = id;

            if (EntityManager.IsAlive)
                World = EntityManager.Worlds.Data[worldIndex];
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
        public void AddHecsComponent(IComponent component, IEntity owner = null, bool silently = false)
        {
            if (component == null)
                throw new Exception($"compontent is null " + ID);

            if (component.ComponentsMask.TypeHashCode == 0)
                component.ComponentsMask = TypesMap.GetComponentInfo(component).ComponentsMask;

            if (components[component.ComponentsMask.Index] != null)
                return;

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

            if (IsInited)
            {
                if (component is IInitable initable)
                    initable.Init();

                if (component is IAfterEntityInit afterEntityInit)
                    afterEntityInit.AfterEntityInit();
            }

            ComponentsMask.AddMask(component.ComponentsMask.Index);

            if (!silently && IsInited)
            {
                World.AddOrRemoveComponent(component, true);
                TypesMap.RegisterComponent(component.ComponentsMask.Index, component.Owner, true);
            }
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
                throw new InvalidOperationException($"Entity already inited: {GUID}");

            if (World == null)
                World = EntityManager.Default;

            InitComponentsAndSystems();

            if (needRegister)
                EntityManager.RegisterEntity(this, true);

            AfterInit();
        }

        public void AfterInit()
        {
            foreach (var c in components)
                if (c != null)
                    if (c is IAfterEntityInit afterEntityInit)
                        afterEntityInit.AfterEntityInit();

            foreach (var s in systems)
                if (s is IAfterEntityInit afterSysEntityInit)
                    afterSysEntityInit.AfterEntityInit();
        }

        public void InitComponentsAndSystems(bool needRegister = true)
        {
            //ComponentsMask = HECSMask.Empty;

            foreach (var component in components)
            {
                if (component is IInitable init)
                    init.Init();
            }

            foreach (var sys in systems)
            {
                if (needRegister)
                    RegisterService.RegisterSystem(sys);

                sys.InitSystem();
            }

            IsInited = true;
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
                RegisterService.RegisterSystem(system);
                system.InitSystem();

                if (system is IAfterEntityInit afterSysEntityInit)
                    afterSysEntityInit.AfterEntityInit();
            }
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
            if (!EntityManager.IsAlive)
                return;

            EntityManager.RegisterEntity(this, false);
            IsAlive = false;
            IsPaused = true;

            foreach (var s in systems.ToArray())
            {
                RemoveHecsSystem(s);
            }

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

            foreach (var s in systems)
            {
                RegisterService.UnRegisterSystem(s);
                s.Dispose();
            }

            systems.Clear();
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

        public void SetWorld(World world)
        {
            RemoveComponentsFromWorld();
            World = world;
            AddComponentsToWorld();
        }

        public void SetWorld(int world)
        {
            RemoveComponentsFromWorld();
            World = EntityManager.Worlds.Data[world];
            AddComponentsToWorld();
        }

        public void InitWorld(World world)
        {
            World = world;
        }

        private void RemoveComponentsFromWorld()
        {
            foreach (var c in components)
                World.AddOrRemoveComponent(c, false);
        }

        private void AddComponentsToWorld()
        {
            foreach (var c in components)
                World.AddOrRemoveComponent(c, true);
        }
    }

    public interface IChangeWorld
    {
        void SetWorld(World world);
        void SetWorld(int world);
    }
}