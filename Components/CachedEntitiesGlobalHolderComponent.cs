using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Helpers, Doc.HECS, Doc.Holder, "here we hold cached entities, when we need create some entity from container, and reuse it, entity should be inited")]
    public sealed partial class CachedEntitiesGlobalHolderComponent : BaseComponent, IWorldSingleComponent , IDisposable
    {
       private Dictionary<int, Entity> cachedEntities = new Dictionary<int, Entity>(8);

        public void Dispose()
        {
            cachedEntities.Clear();
        }

        public bool TryGetEntity(int containerIndex, out Entity entity)
        {
           return cachedEntities.TryGetValue(containerIndex, out entity);
        }

        public Entity AddEntityToCache(Entity entity) 
        { 
            cachedEntities.Add(entity.GetComponent<ActorContainerID>().ContainerIndex, entity);
            return entity;
        }
    }
}