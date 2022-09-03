namespace HECSFramework.Core
{
    public abstract class BaseComponent : IComponent
    {
        public HECSMask ComponentsMask { get; set; }
        public IEntity Owner { get; set; }
        public bool IsAlive { get; set; } = true;
        public bool IsRegistered { get; private set; }

        public BaseComponent()
        {
            ConstructorCall();
        }

        public void SetIsRegistered()
        {
            IsRegistered = true;
        }

        public void UnRegister()
        {
            IsRegistered = false;
        }

        protected virtual void ConstructorCall()
        {
        }

        public int GetTypeHashCode => ComponentsMask.TypeHashCode;
    }
}