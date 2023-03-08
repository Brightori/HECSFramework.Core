namespace HECSFramework.Core
{
    public sealed class UniversalReactLocalT<T> : UniversalReact
    {
        private HECSList<IReactGenericLocalComponent<T>> reacts = new HECSList<IReactGenericLocalComponent<T>>(8);

        private World world;

        public UniversalReactLocalT(World world)
        {
            this.world = world;
        }

        public override void React(IComponent component, bool added)
        {
            if (component is T needed)
            {
                foreach (var r in reacts)
                    r?.ComponentReactLocal(needed, added);
            }
        }

        public void AddListener(IReactGenericLocalComponent<T> listener, bool add)
        {
            if (add)
                reacts.Add(listener);
            else
                reacts.RemoveSwap(listener);
        }

        public override void Dispose()
        {
            reacts.Clear();
            world = null;
        }
    }
}
