namespace HECSFramework.Core
{
    public abstract partial class BaseComponent : IComponent 
    {
        private int getTypeHashCode;

        public bool IsAlive { get; set; }
        public Entity Owner { get; set; }
        public int GetTypeHashCode 
        {
            get
            {
                if (getTypeHashCode == 0)
                    getTypeHashCode = TypesMap.GetComponentInfo(this).ComponentsMask.TypeHashCode;

                
                return getTypeHashCode;
            }
        }

        public BaseComponent()
        {
            ConstructorCall();
        }

        protected virtual void ConstructorCall()
        {
        }

        public virtual void BeforeDispose()
        {
        }
    }
}