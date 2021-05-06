using HECSFramework.Core;
using System;
using System.Collections.Generic;

namespace HECSFramework.Core 
{
    public interface IEntity : IDisposable, IHavePause, IEquatable<IEntity>
    {
        ICommandService EntityCommandService { get; }

        int WorldId { get; }
        World World { get; }


        Guid GUID { get; }
        ref HECSMask ComponentsMask { get; }

        IComponent[] GetAllComponents { get; }
        List<ISystem> GetAllSystems { get; }
        ComponentContext ComponentContext { get; }

        bool TryGetHecsComponent<T>(HECSMask mask, out T component) where T : IComponent;
        T GetOrAddComponent<T>(IEntity owner = null) where T : class, IComponent;
        void AddOrReplaceComponent(IComponent component, IEntity owner = null, bool silently = false);
        void AddHecsComponent(IComponent component, IEntity owner = null, bool silently = false);
        
        void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem;

        void RemoveHecsComponent(IComponent component);
        void RemoveHecsComponent(HECSMask component);
        void RemoveHecsComponent<T>() where T: IComponent;
        
        void RemoveHecsSystem(ISystem system);
        void Command<T>(T command) where T : ICommand;

        bool TryGetSystem<T>(out T system) where T : ISystem;
  
        void Init();
        void Init(int worldIndex);
        void InjectEntity(IEntity entity, IEntity owner = null, bool additive = false);
        
        void GenerateGuid();
        void SetGuid(Guid guid);

        bool ContainsMask(ref HECSMask mask);
        bool ContainsMask<T>() where T: IComponent;

        void HecsDestroy();

        string ID { get; }
        string ContainerID { get; }

        bool IsInited { get; }
        bool IsAlive { get; }
        bool IsPaused { get; }
    }
}

