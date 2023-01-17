using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HECSFramework.Core.Helpers;
using static UnityEngine.UI.GridLayoutGroup;

namespace HECSFramework.Core
{
    public partial class World
    {
        public IEntity[] Entities;
        private HECSList<IReactEntity> reactEntities = new HECSList<IReactEntity>();
        
        //here we have free entities for pooling|using
        private Queue<int> freeIndices = new Queue<int>();

        //dirty entities should be processing 
        private HashSet<int> dirtyEntities = new HashSet<int>(32);
        private HECSList<RegisterEntity> registerEntity = new HECSList<RegisterEntity>(32);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterEntity(IEntity entity, bool isAdded)
        {
            if (isAdded)
                ProcessAddedEntity(entity);
            else
                ProcessRemovedEntity(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessAddedEntity(IEntity entity)
        {
            if (entity.EntityIndex == -1)
            {
                ref var getEntity = ref PullEntity();
                entity.SetID(getEntity.EntityIndex);
                getEntity = entity;
            }

            registerEntity.Add(new Core.RegisterEntity { Entity = entity, IsAdded = true });
            RegisterDirtyEntity(entity.EntityIndex);

            foreach (var c in entity.Components)
            {
                var icomponent = componentProvidersByTypeIndex[c].GetIComponent(entity.EntityIndex);

                //here we add unity part
                ComponentAdditionalProcessing(icomponent, entity);

                if (icomponent is IInitable initable)
                    initable.Init();
            }

            foreach (var s in entity.GetAllSystems)
            {
                SystemAdditionalProcessing(s, entity);

                if (s is IInitable initable)
                    s.InitSystem();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void ComponentAdditionalProcessing(IComponent component, IEntity owner);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void SystemAdditionalProcessing(ISystem system, IEntity owner);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessRemovedEntity(IEntity entity)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MigrateEntityToWorld(IEntity entity)
        {
            if (entity.World == this)
                return;
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

        public ComponentProvider GetComponentProvider(int index) => componentProvidersByTypeIndex[index];


        public void AddEntityListener(IReactEntity reactEntity, bool add)
        {
            reactEntities.AddOrRemoveElement(reactEntity, add);
        }

        public void ReleaseEntity(IEntity entity)
        {
            if (entity.IsAlive)
                entity.Dispose();

            freeIndices.Enqueue(entity.EntityIndex);
        }
    }

    public struct RegisterEntity 
    {
        public IEntity Entity;
        public bool IsAdded;
    }
}
