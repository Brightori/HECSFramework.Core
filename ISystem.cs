using System;

namespace HECSFramework.Core 
{
    public abstract class BaseSystem : ISystem
    {
        private int typeHashCode = -1;
        public IEntity Owner { get; set; }
        public Guid SystemGuid { get; } = Guid.NewGuid();

        public bool IsDisposed { get; protected set; }

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

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
        }

        public abstract void InitSystem();
    }

    public interface ISystem : IDisposable, IHaveOwner
    {
        Guid SystemGuid { get; }
        void InitSystem();
        int GetTypeHashCode { get; }
        bool IsDisposed { get; }
    }
   
    public interface IHavePause
    {
        void Pause();
        void UnPause();
    }
}