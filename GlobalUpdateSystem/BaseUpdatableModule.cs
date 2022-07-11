using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public abstract class BaseUpdatableModule<T> : IDisposable, IRegisterUpdate<T> where T : IRegisterUpdatable
    {
        protected sealed class UpdateWithOwnerContainer
        {
            public IEntity Entity;
            public T Updatable;

            public UpdateWithOwnerContainer(IEntity entity, T updatable)
            {
                Entity = entity;
                Updatable = updatable;
            }
        }

        protected readonly ConcurrencyList<T> updatables = new ConcurrencyList<T>(128);
        protected readonly ConcurrencyList<UpdateWithOwnerContainer> updateOnEntities = new ConcurrencyList<UpdateWithOwnerContainer>(128);

        private Queue<UpdateWithOwnerContainer> addWithOwnersQueue = new Queue<UpdateWithOwnerContainer>(16);
        private Queue<T> removeWithOwnersQueue = new Queue<T>(16);

        private Queue<T> addUpdatablesQueue = new Queue<T>(16);
        private Queue<T> removeUpdatablesQueue = new Queue<T>(16);

        protected bool IsDirty;

        public void Register(T updatable, bool add)
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
                if (updatable is IHaveOwner)
                    removeWithOwnersQueue.Enqueue(updatable);
                else
                    removeUpdatablesQueue.Enqueue(updatable);
            }

            IsDirty = true;
        }

        protected void ProcessAddRemove()
        {
            if (IsDirty)
            {
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

                while (removeWithOwnersQueue.Count > 0)
                {
                    var remove = removeWithOwnersQueue.Dequeue();

                    var count = updateOnEntities.Count;

                    for (int i = 0; i < count; i++)
                    {
                        var update = updateOnEntities.Data[i];

                        if (update.Updatable.Equals(remove))
                        {
                            updateOnEntities.RemoveAt(i);
                            break;
                        }
                    }
                }

                while (removeUpdatablesQueue.Count > 0)
                {
                    var remove = removeUpdatablesQueue.Dequeue();
                    updatables.Remove(remove);
                }

                AfterAddOrRemove();
                IsDirty = false;
            }
        }

        protected abstract void AfterAddOrRemove();

        public void Dispose()
        {
            updatables.Clear();
            updateOnEntities.Clear();

            addWithOwnersQueue.Clear();
            removeWithOwnersQueue.Clear();

            addUpdatablesQueue.Clear();
            removeUpdatablesQueue.Clear();
        }
    }
}
