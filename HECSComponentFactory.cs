using System;

namespace HECSFramework.Core
{
    public partial class HECSFactory : IHECSFactory
    {
        private Func<int, IComponent> getComponentFromFactoryByHash;
        private Func<int, ISystem> getSystemFromFactoryByHash;

        T IHECSFactory.GetComponentFromFactory<T>()
        {
            var hash = TypesMap.GetHashOfComponentByType(typeof(T));
            return (T)GetComponentFromFactory(hash);
        }

        public IComponent GetComponentFromFactory(int hashCodeType) => getComponentFromFactoryByHash(hashCodeType);

        public ISystem GetSystemFromFactory(int hashCodeType) => getSystemFromFactoryByHash(hashCodeType);
    }
}