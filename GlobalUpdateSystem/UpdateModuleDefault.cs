using System.Collections.Generic;

namespace HECSFramework.Core
{
    public sealed class UpdateModuleDefault : IUpdatable, IRegisterUpdate<IUpdatable>
    {
        private sealed class UpdateWithOwnerContainer
        {
            public IEntity Entity;
            public IUpdatable Updatable;

            public UpdateWithOwnerContainer(IEntity entity, IUpdatable updatable)
            {
                Entity = entity;
                Updatable = updatable;
            }
        }

        private readonly List<IUpdatable> updatables = new List<IUpdatable>(128);
        private readonly List<UpdateWithOwnerContainer> updateOnEntities = new List<UpdateWithOwnerContainer>(128);

        private Queue<UpdateWithOwnerContainer> addWithOwnersQueue = new Queue<UpdateWithOwnerContainer>(16);
        private Queue<IHaveOwner> removeWithOwnersQueue = new Queue<IHaveOwner>(16);
   
        private Queue<IUpdatable> addUpdatablesQueue = new Queue<IUpdatable>(16);
        private Queue<IUpdatable> removeUpdatablesQueue = new Queue<IUpdatable>(16);

        public void Register(IUpdatable updatable, bool add)
        {
            if (add)
            {
                if (updatable is IHaveOwner property)
                    addWithOwnersQueue.Enqueue(new UpdateWithOwnerContainer(property.Owner, updatable));
                else
                    addUpdatablesQueue.Enqueue(updatable);
            }
            else
            {

                if (updatable is IHaveOwner property)
                    removeWithOwnersQueue.Enqueue(property);
                else
                    removeUpdatablesQueue.Enqueue(updatable);
            }
        }

        private void UpdateWithOwners()
        {
            var count = updateOnEntities.Count;

            for (int i = 0; i < count; i++)
            {
                if (!updateOnEntities[i].Entity.IsAlive || updateOnEntities[i].Entity.IsPaused) continue;
                updateOnEntities[i].Updatable.UpdateLocal();
            }
        }

        private void ProcessAddRemove()
        {
            while (removeWithOwnersQueue.Count > 0)
            {
                var remove = removeWithOwnersQueue.Dequeue();

                var count = updateOnEntities.Count;
                
                for (int i = 0; i < count; i++)
                {
                    var update = updateOnEntities[i];

                    if (update.Entity == remove)
                    {
                        updateOnEntities.RemoveAt(i);
                        break;
                    }
                }
            }

            while (removeUpdatablesQueue.Count > 0)
            {
                var remove = removeWithOwnersQueue.Dequeue();

                var count = updatables.Count;

                for (int i = 0; i < count; i++)
                {
                    var update = updatables[i];

                    if (update == remove)
                    {
                        updatables.RemoveAt(i);
                        break;
                    }
                }
            }

            while (addWithOwnersQueue.Count > 0)
            {
                var add = addWithOwnersQueue.Dequeue();
                updateOnEntities.Add(add);
            }

            while (addUpdatablesQueue.Count > 0)
            {
                var add = addUpdatablesQueue.Dequeue();
                updatables.Add(add);
            }
        }

        public void UpdateLocal()
        {
            ProcessAddRemove();
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