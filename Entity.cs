using Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core
{
    [Serializable]
    public partial class Entity : IEntity, IChangeWorldIndex
    {
        public int WorldId { get; private set; } = 0;
        public World World { get; private set; }

        public Guid GUID { get; private set; }
        public ComponentContext ComponentContext { get; } = new ComponentContext();
        public string ID { get; protected set; }

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
        private HECSMask componentsMask;

        public ICommandService EntityCommandService { get; } = new EntityCommandService();

        public bool IsInited { get; protected set; }
        public bool IsAlive { get; private set; } = true;
        public bool IsPaused { get; private set; }
        public bool IsLoaded { get; set; }

        public ref HECSMask ComponentsMask => ref componentsMask;

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

        public Entity() { }

        public Entity(string id, int worldIndex)
        {
            ID = id;
            WorldId = worldIndex;
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

            if (TypesMap.GetComponentInfo(component.GetTypeHashCode, out var info))
                component.ComponentsMask = info.ComponentsMask;
            else
                throw new Exception("we dont have needed type in TypesMap, u need to run codogen or check this type manualy" + component.GetType().Name);

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
            ComponentContext.AddComponent(component);

            if (IsInited)
            {
                if (component is IInitable initable)
                    initable.Init();

                if (component is IAfterEntityInit afterEntityInit)
                    afterEntityInit.AfterEntityInit();
            }

            ComponentsMask += component.ComponentsMask;

            if (!silently && IsInited)
                EntityManager.AddOrRemoveComponent(component, true);
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

        public void Init(int worldIndex)
        {
            WorldId = worldIndex;
            Init();
        }

        public void Init()
        {
            SetWorld();
            InitComponentsAndSystems();
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

        public void InitComponentsAndSystems()
        {
            //ComponentsMask = HECSMask.Empty;

            foreach (var component in components)
            {
                if (component is IInitable init)
                    init.Init();
            }

            foreach (var sys in systems)
            {
                sys.InitSystem();
                RegisterService.RegisterSystem(sys);
            }

            IsInited = true;
        }

        public void Command<T>(T command) where T : ICommand
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

            components[component.ComponentsMask.Index] = null;
            ComponentContext.RemoveComponent(component);
            ComponentsMask -= component.ComponentsMask;
            World.AddOrRemoveComponentEvent(component, false);
        }

        protected virtual void Reset()
        {
            GenerateId();
        }

        public virtual void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem
        {
            if (owner == null)
                system.Owner = this;
            else
            {
                system.Owner = owner;
                SystemAdditionalProcessing(system, owner);
            }

            if (systems.Any(x => x.GetTypeHashCode == system.GetTypeHashCode))
                throw new Exception($"we alrdy have this type of system  + { system.ToString() } {ID}");

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

        public virtual void Dispose()
        {
            IsAlive = false;

            foreach (var s in systems)
                s.Dispose();

            foreach (var c in components)
            {
                if (c is IDisposable disposable)
                    disposable.Dispose();
            }

            systems.Clear();
            Array.Clear(components, 0, components.Length);
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

        public virtual void RemoveHecsSystem(ISystem system)
        {
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
            return componentsMask.Contain(ref mask);
        }

        public void HecsDestroy()
            => Dispose();

        public void SetGuid(Guid guid)
        {
            GUID = guid;
        }

        public void SetWorld()
        {
            World = EntityManager.Worlds[WorldId];
        }

        void IChangeWorldIndex.SetWorldIndex(int index)
        {
            WorldId = index;
        }

        public void AddOrReplaceComponent(IComponent component, IEntity owner = null, bool silently = false)
        {
            if (GetAllComponents[component.ComponentsMask.Index] != null)
                RemoveHecsComponent(component.ComponentsMask);

            AddHecsComponent(component, owner, silently);
        }

        partial void ComponentAdditionalProcessing(IComponent component, IEntity owner);
        partial void SystemAdditionalProcessing(ISystem system, IEntity owner);

        public bool ContainsMask<T>() where T : IComponent
        {
            var index = TypesMap.GetIndexByType<T>();
            return components[index] != null;
        }

        public void RemoveHecsComponent<T>() where T: IComponent
        {
            var index = TypesMap.GetIndexByType<T>();
            if (components[index] != null)
                RemoveHecsComponent(components[index]);
        }
    }

    public interface IChangeWorldIndex
    {
        void SetWorldIndex(int index);
    }
}