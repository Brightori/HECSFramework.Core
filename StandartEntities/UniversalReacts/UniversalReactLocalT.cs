using System.Collections.Generic;

namespace HECSFramework.Core
{
    public sealed class UniversalReactLocalT<T> : UniversalReact, IPriorityUpdatable
    {
        private HashSet<IReactGenericLocalComponent<T>> reacts = new HashSet<IReactGenericLocalComponent<T>>(2);
        private Queue<T> addedQueue = new Queue<T>(2);

        public int Priority { get; } = -1;

        private World world;

        public UniversalReactLocalT(World world)
        {
            this.world = world;
            world.GlobalUpdateSystem.Register(this, true);
        }

        public override void React(IComponent component, bool added)
        {
            if (component is T needed)
            {
                if (added)
                    addedQueue.Enqueue(needed);
                else
                {
                    foreach (var r in reacts)
                        r?.ComponentReactLocal(needed, false);
                }
            }
        }

        public void AddListener(IReactGenericLocalComponent<T> listener, bool add)
        {
            if (add)
                reacts.Add(listener);
            else
                reacts.Remove(listener);
        }

        public override void Dispose()
        {
            reacts.Clear();
            world.GlobalUpdateSystem.Register(this, false);
            world = null;
        }

        public void PriorityUpdateLocal()
        {
            while (addedQueue.TryDequeue(out var component))
            {
                foreach (var r in reacts)
                    r?.ComponentReactLocal(component, true);
            }
        }

        public override void ForceReact()
        {
            PriorityUpdateLocal();
        }
    }
}
