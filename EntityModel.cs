using Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core
{
    public sealed partial class EntityModel : IEntity
    {
        private const string DefaultContainerName = "Default";

        public EntityLocalCommandService EntityCommandService => null;
        public int WorldId { get; private set; }
        public World World { get; private set; }
        public Guid GUID { get; private set; }
        public HECSMultiMask ComponentsMask { get; } = new HECSMultiMask();
        public IComponent[] GetAllComponents { get; } = new IComponent[TypesMap.SizeOfComponents];
        public List<ISystem> GetAllSystems { get; } = new List<ISystem>();
        public ComponentContext ComponentContext { get; } = new ComponentContext();
        public string ID => ContainerID;

        private HECSMask ActorContainerMask = HMasks.GetMask<ActorContainerID>();

        public string ContainerID 
        {
            get
            {
                if (TryGetHecsComponent(ActorContainerMask, out ActorContainerID actorContainerID))
                    return actorContainerID.ID;
                else
                    return DefaultContainerName;
            }
        }

        public bool IsInited => false;
        public bool IsAlive => false;
        public bool IsPaused => true;

        public LocalComponentListenersService RegisterComponentListenersService { get; } 

        public EntityModel(int index, string ID)
        {
            WorldId = index;
            World = EntityManager.Worlds.Data[index];
        }

        public T AddHecsComponent<T>(T component, IEntity owner = null, bool silently = false) where T: IComponent
        {
            if (component == null)
                throw new Exception($"compontent is null " + ID);

            if (TypesMap.GetComponentInfo(component.GetTypeHashCode, out var info))
                component.ComponentsMask = info.ComponentsMask;
            else
                throw new Exception("we dont have needed type in TypesMap, u need to run codogen or check this type manualy" + component.GetType().Name);

            if (GetAllComponents[component.ComponentsMask.Index] != null)
                return (T)GetAllComponents[component.ComponentsMask.Index];

            component.Owner = this;

            GetAllComponents[component.ComponentsMask.Index] = component;
            TypesMap.SetComponent(this, component);
            component.IsAlive = true;

            if (component is IInitable initable)
                initable.Init();

            ComponentsMask.AddMask(component.ComponentsMask.Index);
            return component;
        }

        public void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem
        {
            if (GetAllSystems.Any(x => x.GetTypeHashCode == system.GetTypeHashCode))
                return;

            system.Owner = this;
            GetAllSystems.Add(system);
        }

        public void AddOrReplaceComponent(IComponent component, IEntity owner = null, bool silently = false)
        {
            var index = TypesMap.GetComponentInfo(component);
            if (GetAllComponents[index.ComponentsMask.Index] != null)
                RemoveHecsComponent(index.ComponentsMask);

            AddHecsComponent(component, owner, silently);
        }

        public void Command<T>(T command) where T : struct, ICommand
        {
            return;
        }

        public bool ContainsMask(ref HECSMask mask)
        {
            return GetAllComponents[mask.Index] != null;
        }

        public bool ContainsMask(HECSMultiMask mask)
        {
            return ComponentsMask.Contains(mask);
        }

        public bool ContainsMask<T>() where T : IComponent
        {
            var index = TypesMap.GetIndexByType<T>();
            return GetAllComponents[index] != null;
        }

        public void Dispose()
        {
            if (!EntityManager.IsAlive)
                return;

            EntityManager.RegisterEntity(this, false);

            for (int i = 0; i < GetAllComponents.Length; i++)
            {
                IComponent c = GetAllComponents[i];

                if (c != null)
                    RemoveHecsComponent(c);
            }

            Array.Clear(GetAllComponents, 0, GetAllComponents.Length);
        }

        public bool Equals(IEntity other)
        {
            return other.GUID == GUID;
        }

        public void GenerateGuid()
        {
            GUID = Guid.NewGuid();
        }

        public IEnumerable<T> GetComponentsByType<T>()
        {
            for (int i = 0; i < GetAllComponents.Length; i++)
            {
                if (GetAllComponents[i] != null && GetAllComponents[i] is T needed)
                    yield return needed;
            }
        }

        public void HecsDestroy()
        {
            Dispose();
        }

        public void Init(bool needRegister = true)
        {
            Init(WorldId, needRegister);
        }

        public void Init(int worldIndex, bool needRegister = true)
        {
            WorldId = worldIndex;
            World = EntityManager.Worlds.Data[worldIndex];

            foreach (var index in ComponentsMask.CurrentIndexes)
            {
                var component = GetAllComponents[index];

                if (component is IAfterEntityInit afterEntityInit)
                    afterEntityInit.AfterEntityInit();
            }
        }

        public void InjectEntity(IEntity entity, IEntity owner = null, bool additive = false)
        {
            throw new Exception("Это ентити модель, в ней только данные, сюда нельзя инжектить ентити");
        }

        public void Pause()
        {
            return;
        }

        public void RemoveHecsComponent(IComponent component)
        {
            if (component == null)
                return;

            if (component is IDisposable disposable)
                disposable.Dispose();

            GetAllComponents[component.ComponentsMask.Index] = null;
            TypesMap.RemoveComponent(this, component);
            ComponentsMask.RemoveMask(component.ComponentsMask.Index);

            component.IsAlive = false;
        }

        public void RemoveHecsComponent(HECSMask component)
        {
            var needed = GetAllComponents[component.Index];
            RemoveHecsComponent(needed);
        }

        public void RemoveHecsComponent<T>() where T : IComponent
        {
            var index = TypesMap.GetIndexByType<T>();
            if (GetAllComponents[index] != null)
                RemoveHecsComponent(GetAllComponents[index]);
        }

        public void RemoveHecsSystem(ISystem system)
        {
            return;
        }

        public void SetGuid(Guid guid)
        {
            GUID = guid;
        }

        public bool TryGetHecsComponent<T>(HECSMask mask, out T component) where T : IComponent
        {
            var needed = GetAllComponents[mask.Index];

            if (needed != null && needed is T cast)
            {
                component = cast;
                return true;
            }

            component = default;
            return false;
        }

        public bool TryGetHecsComponent<T>(out T component) where T : IComponent
        {
            var index = TypesMap.GetIndexByType<T>();

            if (GetAllComponents[index] != null)
            {
                component = (T)GetAllComponents[index];
                return true;
            }

            component = default;
            return false;
        }

        public bool TryGetSystem<T>(out T system) where T : ISystem
        {
            system = default;
            return false;
        }

        public void UnPause()
        {
            return;
        }

        T IEntity.GetOrAddComponent<T>(IEntity owner)
        {
            var index = TypesMap.GetIndexByType<T>();
            var needed = GetAllComponents[index];

            if (needed != null)
                return (T)needed;

            var newComp = TypesMap.GetComponentFromFactory<T>();
            AddHecsComponent(newComp, owner);
            return newComp;
        }

        public bool ContainsAnyFromMask(FilterMask mask)
        {
            return ComponentsMask.ContainsAny(mask);
        }

        public bool ContainsAnyFromMask(HECSMultiMask mask)
        {
            return ComponentsMask.ContainsAny(mask);
        }

        public bool ContainsMask(FilterMask mask)
        {
            return ComponentsMask.Contains(mask);
        }

        public bool RemoveHecsSystem<T>() where T : ISystem
        {
            return false;
        }

        //we not support behaviour for entity model
        public void MigrateEntityToWorld(World world, bool needInit = true)
        {
            throw new NotImplementedException();
        }

        //we not support behaviour for entity model
        public void Inject(List<IComponent> components, List<ISystem> systems, bool isAdditive = false, IEntity owner = null)
        {
            throw new NotImplementedException();
        }
    }
}