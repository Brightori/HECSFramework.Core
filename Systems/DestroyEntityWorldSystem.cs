using Commands;
using Components;
using HECSFramework.Core;
using System.Collections.Generic;

namespace Systems
{
    [Documentation(Doc.GameLogic, "Эта система живет в самом мире, отвечает за то что после всех апдейтов вызовется эта система, и почистит ентити которые мы просим удалить")]
    public sealed partial class DestroyEntityWorldSystem : BaseSystem, IReactGlobalCommand<DestroyEntityWorldCommand>
    {
        private Queue<Entity> entitiesForDelete = new Queue<Entity>(8);

        public override void InitSystem()
        {
            Owner.World.GlobalUpdateSystem.FinishUpdate += React;
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
                        if (entity.ContainsMask<PoolableTagComponent>())
                            actorProviderComponent.Actor.RemoveActorToPool();
                        else
                            actorProviderComponent.Actor.HecsDestroy();

                        continue;
                    }
                    entity.Dispose();
                }
            }
        }

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
}

namespace Commands
{
    public struct DestroyEntityWorldCommand : IGlobalCommand
    {
        public Entity Entity;
    }
}