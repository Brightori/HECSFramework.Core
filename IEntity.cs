using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public partial interface IEntity : IDisposable, IHavePause, IEquatable<IEntity>
    {
        EntityLocalCommandService EntityCommandService { get; }

        int WorldId { get; }
        World World { get; }


        Guid GUID { get; }
        HECSMultiMask ComponentsMask { get; }

        IComponent[] GetAllComponents { get; }
        List<ISystem> GetAllSystems { get; }
        ComponentContext ComponentContext { get; }
        LocalComponentListenersService RegisterComponentListenersService { get; }

        IEnumerable<T> GetComponentsByType<T>();

        /// <summary>
        /// это быстрый способ для рантайма
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mask">сюда берем маску из HMask. </param>
        /// <param name="component"></param>
        /// <returns></returns>
        bool TryGetHecsComponent<T>(HECSMask mask, out T component) where T : IComponent;

        /// <summary>
        /// это медленный способ, его нужно использовать при ините
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        bool TryGetHecsComponent<T>(out T component) where T : IComponent;

        T GetOrAddComponent<T>(IEntity owner = null) where T : class, IComponent;
        void AddOrReplaceComponent(IComponent component, IEntity owner = null, bool silently = false);
        T AddHecsComponent<T>(T component, IEntity owner = null, bool silently = false) where T: IComponent;
        
        void AddHecsSystem<T>(T system, IEntity owner = null) where T : ISystem;

        void RemoveHecsComponent(IComponent component);
        void RemoveHecsComponent(HECSMask component);
        void RemoveHecsComponent<T>() where T: IComponent;
        
        void RemoveHecsSystem(ISystem system);
        bool RemoveHecsSystem<T>() where T: ISystem;

        void Command<T>(T command) where T : struct, ICommand;

        bool TryGetSystem<T>(out T system) where T : ISystem;
  
        void Init(bool needRegister = true);
        void Init(int worldIndex, bool needRegister = true);

        void InjectEntity(IEntity entity, IEntity owner = null, bool additive = false);
        void Inject(List<IComponent> components, List<ISystem> systems, bool isAdditive = false, IEntity owner = null);
        
        void GenerateGuid();
        void SetGuid(Guid guid);

        bool ContainsMask(ref HECSMask mask);
        bool ContainsMask(FilterMask mask);
        bool ContainsAnyFromMask(FilterMask mask);
        bool ContainsMask(HECSMultiMask mask);
        bool ContainsAnyFromMask(HECSMultiMask mask);

        bool ContainsMask<T>() where T: IComponent;

        void HecsDestroy();

        void MigrateEntityToWorld(World world, bool needInit = true);

        string ID { get; }
        string ContainerID { get; }

        bool IsInited { get; }
        bool IsAlive { get; }
        bool IsPaused { get; }
    }
}

