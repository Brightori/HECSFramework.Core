using HECSFramework.Core.Helpers;
using System;

namespace HECSFramework.Core
{
    public class EntityService : IDisposable
    {
        public ConcurrencyList<IEntity> Entities { get; private set; } = new ConcurrencyList<IEntity>();

        private ConcurrencyList<IReactEntity> reactEntities = new ConcurrencyList<IReactEntity>();

        public void RegisterEntity(IEntity entity, bool isAdded)
        {
            if (isAdded)
                Entities.Add(entity);
            else
                Entities.Remove(entity);

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
                reactEntities.Data[i].EntityReact(entity, isAdded);
        }

        public void Dispose()
        {
            if (!EntityManager.IsAlive)
                return;

            foreach (var e in Entities.ToArray())
                e?.Dispose();
                
            Entities.Clear();
        }
    }
}