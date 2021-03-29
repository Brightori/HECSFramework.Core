using HECSFramework.Core.Helpers;
using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class EntityService : IDisposable
    {
        private HashSet<IEntity> entities = new HashSet<IEntity>();
        private IEntity[] entitiesFastIterate = new IEntity[128];
        private int currentIndex = 0;

        public int Count => currentIndex;

        public IEntity[] Entities => entitiesFastIterate;

        private List<IReactEntity> reactEntities = new List<IReactEntity>(16);

        public void RegisterEntity(IEntity entity, bool isAdded)
        {
            if (isAdded)
            {
                lock (entities)
                {
                    lock (entitiesFastIterate)
                    {
                        if (currentIndex >= entitiesFastIterate.Length)
                            Array.Resize(ref entitiesFastIterate, entitiesFastIterate.Length * 2);

                        if (entities.Add(entity))
                        {
                            entitiesFastIterate[currentIndex] = entity;
                            currentIndex++;
                        }
                    }
                }
            }
            else
            {
                if (entities.Remove(entity))
                {
                    lock (entitiesFastIterate)
                    {
                        lock (entities)
                        {
                            currentIndex = 0;

                            foreach (var e in entities)
                            {
                                entitiesFastIterate[currentIndex] = e;
                                currentIndex++;
                            }
                        }
                    }
                }
            }

            ProcessEntityListeners(entity, isAdded);
        }

        public void AddEntityListener(IReactEntity reactEntity, bool add)
        {
            reactEntities.AddOrRemoveElement(reactEntity, add);
        }

        public void ProcessEntityListeners(IEntity entity, bool isAdded)
        {
            var count = reactEntities.Count;

            for (int i = 0; i < count; i++)
                reactEntities[i].EntityReact(entity, isAdded);
        }

        public void Dispose()
        {
            entities.Clear();
            Array.Clear(entitiesFastIterate, 0, currentIndex);
        }
    }
}
