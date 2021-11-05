using Components;
using HECSFramework.Core;
using System.Collections.Generic;

namespace Systems
{
    [Documentation(Doc.Gamelogic, "Эта система живет в самом мире, отвечает за то что после всех апдейтов вызовется эта система, и почистит ентити которые мы просим удалить")]
    public class DestroyEntityWorldSystem : BaseSystem, IReactGlobalCommand<DestroyEntityWorldCommand>
    {
        private Queue<IEntity> entitiesForDelete = new Queue<IEntity>(8); 

        public override void InitSystem()
        {
            Owner.World.GlobalUpdateSystem.FinishUpdate += React;
        }

        private void React()
        {
            while (entitiesForDelete.Count > 0)
            {
                entitiesForDelete.Dequeue().HecsDestroy();
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

    public struct DestroyEntityWorldCommand : IGlobalCommand
    {
        public IEntity Entity;
    }
}