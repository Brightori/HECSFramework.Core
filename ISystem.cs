using System;

namespace HECSFramework.Core 
{
    public abstract partial class BaseSystem : ISystem
    {
        private int typeHashCode = -1;
        public Entity Owner { get; set; }
        public Guid SystemGuid { get; } = Guid.NewGuid();

        public bool IsDisposed { get; set; }

        public int GetTypeHashCode
        {
            get
            {
                if (typeHashCode != -1)
                    return typeHashCode;

                typeHashCode = IndexGenerator.GetIndexForType(GetType());
                return typeHashCode;
            }
        }

        public virtual void BeforeDispose()
        {
        }

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
        }

        public T AsSingle<T>(ref T component) where T: IComponent, IWorldSingleComponent, new()
        {
            component = Owner.World.GetSingleComponent<T>();
            return component;
        }

        public T AsSingleSystem<T>(ref T system) where T : ISystem
        {
            system = Owner.World.GetSingleSystem<T>();
            return system;
        }

        public T AsSingleSystem<T>(ref T system, World world) where T : ISystem
        {
            system = world.GetSingleSystem<T>();
            return system;
        }

        public T AsSingle<T>(ref T component, World world) where T : IComponent, IWorldSingleComponent, new()
        {
            component = world.GetSingleComponent<T>();
            return component;
        }

        public abstract void InitSystem();
    }

    /// <summary>
    /// this interface works only when we dispose entity 
    /// </summary>
    public interface IBeforeEntityDispose
    {
        void BeforeDispose();
    }

    public interface ISystem : IDisposable, IHaveOwner, IBeforeEntityDispose 
    {
        Guid SystemGuid { get; }
        void InitSystem();
        int GetTypeHashCode { get; }
        bool IsDisposed { get; set; }
    }
   
    public interface IHavePause
    {
        void Pause();
        void UnPause();
    }
}