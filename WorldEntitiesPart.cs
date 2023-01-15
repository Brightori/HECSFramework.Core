using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public partial class World
    {
        public IEntity[] Entities;
        private HECSList<IReactEntity> reactEntities = new HECSList<IReactEntity>();
        
        //here we have free entities for pooling|using
        private Queue<int> freeIndices = new Queue<int>();

        //dirty entities should be processing 
        private HECSList<int> dirtyEntities = new HECSList<int>();
        private Dictionary<int, ComponentProvider> componentProvidersByTypeIndex = new Dictionary<int, ComponentProvider>(256);
        private HECSList<EntitiesFilter> entityFilters = new HECSList<EntitiesFilter>(8);

        private void InitStandartEntities()
        {
            GlobalUpdateSystem.FinishUpdate += ProcessDirtyEntities;
        }

        private void ProcessDirtyEntities()
        {
        }

        public ref IEntity PullEntity(string id = "empty")
        {
            if (freeIndices.TryDequeue(out var result))
            {
                if (Entities[result] == null)
                    Entities[result] = new Entity(this,  result, id);


                return ref Entities[result];
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

            dirtyEntities.Add(entity.EntityIndex);

            foreach (var c in entity.Components)
            {

            }
        }

        public void RegisterDirtyEntity(int index)
        {
            dirtyEntities.Add(index);
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
            var currentLenght = Entities.Length;
            Array.Resize(ref Entities, currentLenght * 2);

            for (int i = currentLenght; i < Entities.Length; i++)
            {
                if (Entities[i] == null)
                {
                    Entities[i] = new Entity(this, i);
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
