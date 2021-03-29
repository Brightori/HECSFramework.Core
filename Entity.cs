using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core
{
    [Serializable]
    public class Entity : IEntity
    {
        public int WorldId { get; private set; } = 0;
        public World World { get; private set; }

        public Guid EntityGuid { get; private set; }
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

        public HECSMask ComponentsMask { get => componentsMask; private set => componentsMask = value; }

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

        public void AddHecsComponent(IComponent component, bool silently = false)
        {
            if (component == null)
                throw new Exception($"compontent is null " + ID);

            if (TypesMap.GetComponentInfo(component.GetTypeHashCode, out var info))
                component.ComponentsMask = info.ComponentsMask;
            else
                throw new Exception("we dont have needed type in TypesMap, u need to run codogen or check this type manualy" + component.GetType().Name);


            component.Owner = this;
            components[component.ComponentsMask.Index] = component;
            ComponentContext.AddComponent(component);

            if (IsInited)
            {
                if (component is IInitable initable)
                    initable.Init();
            }

            ComponentsMask += component.ComponentsMask;

            if (!silently)
                EntityManager.AddOrRemoveComponent(component, true);
        }

        public bool TryGetHecsComponent<T>(HECSMask mask, out T component) where T : IComponent
        {
            var needed = components[mask.Index];

            if (needed != null)
            {
                component = (T)needed;
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
                if (c is T needed)
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
            World = EntityManager.Worlds[WorldId];
            InitComponentsAndSystems();
            EntityManager.RegisterEntity(this, true);
        }

        private void InitComponentsAndSystems()
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

        public void AddHecsSystem<T>(T system) where T : ISystem
        {
            system.Owner = this;

            if (systems.Any(x => x is T))
                throw new Exception("we alrdy have this type of system " + system.ToString());

            systems.Add(system);

            if (IsInited)
            {
                RegisterService.RegisterSystem(system);
                system.InitSystem();
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
            EntityGuid = System.Guid.NewGuid();
        }

        public void RemoveHecsSystem(ISystem system)
        {
            RegisterService.UnRegisterSystem(system);
            system.Dispose();
            systems.Remove(system);
        }

        public bool Equals(IEntity other)
        {
            return other.EntityGuid == EntityGuid;
        }

        public T GetOrAddComponent<T>() where T : class, IComponent
        {
            var index = TypesMap.GetIndexByType<T>();
            var needed = components[index];

            if (needed != null)
                return (T)needed;

            var newComp = TypesMap.GetComponentFromFactory<T>();
            AddHecsComponent(newComp);
            return newComp;
        }

        public void RemoveHecsComponent(HECSMask component)
        {
            var needed = components[component.Index];
            RemoveHecsComponent(needed);
        }

        public void InjectEntity(IEntity entity, bool additive = false)
        {
            if (!additive)
                RemoveComponentsAndSystems();

            foreach (var c in entity.GetAllComponents)
                AddHecsComponent(c);

            foreach (var s in entity.GetAllSystems)
                AddHecsSystem(s);

            EntityGuid = entity.EntityGuid;
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

        public void GenerateID()
        {
            EntityGuid = Guid.NewGuid();
        }

        public bool ContainsMask(ref HECSMask mask)
        {
            return componentsMask.Contain(ref mask);
        }

        public void HecsDestroy()
            => Dispose();
    }
}