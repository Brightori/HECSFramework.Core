using System.Collections.Generic;

namespace HECSFramework.Core
{
    public sealed class UniversalReactGlobalT<T> : UniversalReact, IPriorityUpdatable
    {
        private HECSList<IReactGenericGlobalComponent<T>> reacts = new HECSList<IReactGenericGlobalComponent<T>>(16);
        private Queue<T> addedQueue = new Queue<T>(8);

        public int Priority { get; } = -1;

        private World world;

        public UniversalReactGlobalT(World world)
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
                        r?.ComponentReact(needed, false);
                }
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
            world.GlobalUpdateSystem.Register(this, false);
            world = null;
        }

        public void PriorityUpdateLocal()
        {
            var count = reacts.Count;

            while (addedQueue.TryDequeue(out var component))
            {
                for (int i = 0; i < count; i++)
                    reacts.Data[i].ComponentReact(component, true);
            }
        }

        public override void ForceReact()
        {
            PriorityUpdateLocal();
        }
    }
}
