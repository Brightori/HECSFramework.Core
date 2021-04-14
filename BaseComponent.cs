using UnityEngine;

namespace HECSFramework.Core 
{
    public abstract class BaseComponent : IComponent
    {
        private int typeHashCode = -1;
        public HECSMask ComponentsMask { get; set; }
        public IEntity Owner { get; set; }
        
        public BaseComponent()
        {
            ConstructorCall();
        }

        protected virtual void ConstructorCall()
        {
        }

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
    }
}