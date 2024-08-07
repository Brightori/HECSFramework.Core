﻿using Commands;
using Components;
using HECSFramework.Core;
using System.Collections.Generic;

namespace Systems
{
    [Documentation(Doc.GameLogic, "this system provides functionality for destroying entities in the end of frame")]
    public sealed partial class DestroyEntityWorldSystem : BaseSystem, IReactGlobalCommand<DestroyEntityWorldCommand>
    {
        private Queue<Entity> entitiesForDelete = new Queue<Entity>(8);

        public override void InitSystem()
        {
            Owner.World.GlobalUpdateSystem.PreFinishUpdate += React;
        }

        private void React()
        {
            while (entitiesForDelete.Count > 0)
            {
                var entity = entitiesForDelete.Dequeue();

                if (entity.IsAlive())
                {
                    if (entity.TryGetComponent(out ActorProviderComponent actorProviderComponent))
                    {
                        ProcessActor(entity);
                        continue;
                    }
                    entity.Dispose();
                }
            }
        }

        partial void ProcessActor(Entity entity);

        public override void Dispose()
        {
            base.Dispose();
            Owner.World.GlobalUpdateSystem.FinishUpdate -= React;
        }

        public void CommandGlobalReact(DestroyEntityWorldCommand command)
        {
            entitiesForDelete.Enqueue(command.Entity);
        }
    }

    public static class EntityDestroyHelper
    {
        public static void HECSDestroyEndOfFrame(this Entity entity)
        {
            entity.World.Command(new DestroyEntityWorldCommand { Entity = entity });
        }
    }
}

namespace Commands
{
    public struct DestroyEntityWorldCommand : IGlobalCommand
    {
        public Entity Entity;
    }
}