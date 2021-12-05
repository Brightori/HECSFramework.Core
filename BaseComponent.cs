namespace HECSFramework.Core
{
    public abstract class BaseComponent : IComponent
    {
        public HECSMask ComponentsMask { get; set; }
        public IEntity Owner { get; set; }
        public bool IsAlive { get; set; } = true;

        public BaseComponent()
        {
            ConstructorCall();
        }

        protected virtual void ConstructorCall()
        {
        }

        public int GetTypeHashCode => ComponentsMask.TypeHashCode;
    }
}