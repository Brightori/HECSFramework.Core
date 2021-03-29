using System;

namespace HECSFramework.Core
{
    public partial class HECSComponentFactory : IComponentFactory
    {
        private Func<int, IComponent> getComponentFromFactoryByHash;

        T IComponentFactory.GetComponentFromFactory<T>()
        {
            var hash = TypesMap.GetHashOfComponentByType(typeof(T));
            return (T)GetComponentFromFactory(hash);
        }

        public IComponent GetComponentFromFactory(int hashCodeType) => getComponentFromFactoryByHash(hashCodeType);
    }
}