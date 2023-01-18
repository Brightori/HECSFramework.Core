using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public partial interface IEntity : IDisposable, IHavePause, IEquatable<IEntity>
    {
        EntityLocalCommandService EntityCommandService { get; }
        LocalComponentListenersService RegisterComponentListenersService { get; }

        World World { get; }
        int WorldId { get; }

        Guid GUID { get; }
        string ID { get; }
        string ContainerID { get; }
        int Index { get; }
        int Generation { get; set; }

        bool IsInited { get; }
        bool IsAlive { get; }
        bool IsPaused { get; }
        bool IsDirty { get; }

        HashSet<int> Components { get; }
        HECSList<ISystem> GetAllSystems { get; }

        IEnumerable<T> GetComponentsByType<T>();
        
        void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem;
        void RemoveHecsSystem(ISystem system);
        bool RemoveHecsSystem<T>() where T: ISystem;

        void Command<T>(T command) where T : struct, ICommand;

        bool TryGetSystem<T>(out T system) where T : ISystem;
  
        void Init(World world);

        void Inject(List<IComponent> components, List<ISystem> systems, bool isAdditive = false, IEntity owner = null);
        
        void GenerateGuid();
        void SetGuid(Guid guid);
        void SetID(int index);

        bool ContainsMask(HashSet<int> mask);
        bool ContainsMask(int mask);
        bool ContainsAnyFromMask(HashSet<int> mask);

        bool ContainsMask<T>() where T: IComponent, new();
    }
}