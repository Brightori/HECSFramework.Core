namespace HECSFramework.Core
{
    public abstract class BaseComponent : IComponent
    {
        public bool IsAlive { get; set; }
        public IEntity Owner { get; set; }

        public BaseComponent()
        {
            ConstructorCall();
        }

        protected virtual void ConstructorCall()
        {
        }
    }
}