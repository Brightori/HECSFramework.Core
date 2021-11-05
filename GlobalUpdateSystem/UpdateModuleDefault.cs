using System.Collections.Generic;
using HECSFramework.Core.Helpers;

namespace HECSFramework.Core
{
    public class UpdateModuleDefault : IUpdatable, IRegisterUpdate<IUpdatable>
    {
        private readonly List<IUpdatable> updatables = new List<IUpdatable>(64);
        private readonly List<(IEntity, IUpdatable)> updateOnEntities = new List<(IEntity, IUpdatable)>(32);

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
                if (i >= updateOnEntities.Count || !updateOnEntities[i].Item1.IsAlive || updateOnEntities[i].Item1.IsPaused) continue;
                updateOnEntities[i].Item2.UpdateLocal();
            }
        }

        public void UpdateLocal()
        {
            UpdateWithOwners();

            var count = updatables.Count;
            for (int i = 0; i < count; i++)
            {
                IUpdatable updatable = updatables[i];
                updatable.UpdateLocal();
            }
        }
    }
}