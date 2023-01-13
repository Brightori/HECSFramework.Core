using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public partial class World
    {
        private IEntity[] entities;
        private HECSList<IReactEntity> reactEntities = new HECSList<IReactEntity>();
        
        //here we have free entities for pooling|using
        private Queue<int> freeIndices = new Queue<int>();

        //dirty entities should be processing 
        private Queue<int> dirtyEntities = new Queue<int>();
        private Dictionary<int, ComponentProvider> componentProvidersByTypeIndex = new Dictionary<int, ComponentProvider>(256);




        public ref IEntity PullEntity(string id = "empty")
        {
            if (freeIndices.TryDequeue(out var result))
            {
                if (entities[result] == null)
                    entities[result] = new Entity(this,  result, id);


                return ref entities[result];
            }

            else
                return ref ResizeAndReturnEntity();
        }

        public void RegisterEntity(IEntity entity)
        {
            if (entity.EntityIndex == -1)
            {
                ref var getEntity = ref PullEntity();
                entity.SetID(getEntity.EntityIndex);
                getEntity = entity;
            }

            dirtyEntities.Enqueue(entity.EntityIndex);

            foreach (var c in entity.Components)
            {

            }
        }

        public int GetEntityFreeIndex()
        {
            if (freeIndices.TryDequeue(out var result))
            {
                return result;
            }

            ResizeEntitiesList();
            return GetEntityFreeIndex();
        }

        private ref IEntity ResizeAndReturnEntity()
        {
            ResizeEntitiesList();
            return ref PullEntity();
        }

        private void ResizeEntitiesList()
        {
            var currentLenght = entities.Length;
            Array.Resize(ref entities, currentLenght * 2);

            for (int i = currentLenght; i < entities.Length; i++)
            {
                if (entities[i] == null)
                {
                    entities[i] = new Entity(this, i);
                    freeIndices.Enqueue(i);
                }
            }

            foreach (var p in fastComponentProvidersByTypeIndex)
            {
                p.Value.Resize();
            }
        }

        public void RegisterComponentProvider(ComponentProvider componentProvider)
        {
            componentProvidersByTypeIndex.Add(componentProvider.TypeIndexProvider, componentProvider);
        }

        public void ReleaseEntity(IEntity entity)
        {
            if (entity.IsAlive)
                entity.Dispose();

            freeIndices.Enqueue(entity.EntityIndex);
        }
    }
}
