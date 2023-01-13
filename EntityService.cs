using HECSFramework.Core.Helpers;
using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class EntityService : IDisposable
    {
        public HECSList<IEntity> Entities { get; private set; } = new HECSList<IEntity>();

        private HECSList<IReactEntity> reactEntities = new HECSList<IReactEntity>();

        private Stack<int> freeIndeces = new Stack<int>();


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

        public IEntity GetFastEntity()
        {
            if (freeEntities.TryDequeue(out var index))
            {
                ref var fastEntity = ref FastEntities[index];
                fastEntity.IsReady = true;
                RegisterUpdatedFastEntity(ref fastEntity);
                return ref FastEntities[index];
            }

            return ref ResizeAndReturn();
        }

        private ref FastEntity ResizeAndReturn()
        {
            var currentLenght = FastEntities.Length;
            Array.Resize(ref FastEntities, currentLenght * 2);

            for (int i = currentLenght; i < FastEntities.Length; i++)
            {
                if (!FastEntities[i].IsReady)
                {
                    CreateNewEntity(i);
                }
            }

            foreach (var p in componentProvidersByTypeIndex)
            {
                p.Value.Resize();
            }

            return ref GetFastEntity();
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