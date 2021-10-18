using HECSFramework.Core.Helpers;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class UpdateModuleDefault : IUpdatable, IRegisterUpdate<IUpdatable>
    {
        private readonly List<IUpdatable> updatables = new List<IUpdatable>(64);
        private readonly ConcurrencyList<(IEntity, IUpdatable)> updateOnEntities = new ConcurrencyList<(IEntity, IUpdatable)>();

        public void Register(IUpdatable updatable, bool add)
        {
            if (updatable is IHaveOwner entity)
                updateOnEntities.AddOrRemoveElement((entity.Owner, updatable), add);
            else
                updatables.AddOrRemoveElement(updatable, add);
        }

        private void UpdateWithOwners()
        {
            var count = updateOnEntities.Count;

            for (int i = 0; i < count; i++)
            {
                if (!updateOnEntities[i].Item1.IsAlive || updateOnEntities[i].Item1.IsPaused) continue;
                updateOnEntities[i].Item2.UpdateLocal();
            }
        }

        public void UpdateLocal()
        {
            var count = updatables.Count;
            for (int i = 0; i < count; i++)
            {
                IUpdatable updatable = updatables[i];
                updatable.UpdateLocal();
            }

            UpdateWithOwners();
        }
    }
}