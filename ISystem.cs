using System;

namespace HECSFramework.Core 
{
    public abstract class BaseSystem : ISystem
    {
        public IEntity Owner { get; set; }
        public Guid SystemGuid { get; } = Guid.NewGuid();

        public virtual void Dispose()
        {
            Owner = null;
        }

        public abstract void InitSystem();
    }

    public interface ISystem : IDisposable, IHaveOwner
    {
        Guid SystemGuid { get; }
        void InitSystem();
    }
   
    public interface IHavePause
    {
        void Pause();
        void UnPause();
    }
}