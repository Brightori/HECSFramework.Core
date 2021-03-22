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


        Guid EntityGuid { get; }
        HECSMask ComponentsMask { get; }

        IComponent[] GetAllComponents { get; }
        List<ISystem> GetAllSystems { get; }
        ComponentContext ComponentContext { get; }

        bool TryGetHecsComponent<T>(HECSMask mask, out T component) where T : IComponent;
        void GetOrAddComponent<T>(HECSMask mask, out T component) where T : class, IComponent;
        void AddHecsComponent(IComponent component, bool silently = false);
        
        void AddHecsSystem<T>(T system) where T : ISystem;

        void RemoveHecsComponent(IComponent component);
        void RemoveHecsComponent(HECSMask component);
        
        void RemoveHecsSystem(ISystem system);
        void Command<T>(T command) where T : ICommand;

        bool TryGetSystem<T>(out T system) where T : ISystem;
  
        void Init();
        void Init(int worldIndex);
        void InjectEntity(IEntity entity, bool additive = false);
        void GenerateID();

        bool ContainsMask(ref HECSMask mask);

        void HecsDestroy();

        string ID { get; }
        bool IsInited { get; }
        bool IsAlive { get; }
        bool IsPaused { get; }
    }
}

