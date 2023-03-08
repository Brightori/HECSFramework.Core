using System.Collections.Generic;

namespace HECSFramework.Core
{
    public sealed class UniversalReactGlobalT<T> : UniversalReact
    {
        private HECSList<IReactGenericGlobalComponent<T>> reacts = new HECSList<IReactGenericGlobalComponent<T>>(16);

        public UniversalReactGlobalT(World world)
        {
        }

        public override void React(IComponent component, bool added)
        {
            if (component is T needed)
            {
                foreach (var r in reacts)
                    r.ComponentReact(needed, added);
            }
        }

        public void AddListener(IReactGenericGlobalComponent<T> listener, bool add)
        {
            if (add)
                reacts.Add(listener);
            else
                reacts.RemoveSwap(listener);
        }

        public override void Dispose()
        {
            reacts.Clear();
        }
    }
}
