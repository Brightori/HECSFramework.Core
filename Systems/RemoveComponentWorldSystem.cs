using Commands;
using HECSFramework.Core;
using System.Collections.Generic;

namespace Systems
{
    [Documentation(Doc.GameLogic, "Эта система живет в самом мире, отвечает за то что после всех апдейтов вызовется эта система, и почистит ентити которые мы просим удалить")]
    public class RemoveComponentWorldSystem : BaseSystem, IReactGlobalCommand<RemoveHecsComponentWorldCommand>
    {
        private Queue<IComponent> componentsForRemove = new Queue<IComponent>(8);

        public override void InitSystem()
        {
            Owner.World.GlobalUpdateSystem.FinishUpdate += React;
        }

        private void React()
        {
            while (componentsForRemove.Count > 0)
            {
                var component = componentsForRemove.Dequeue();

                if (component != null && component.IsAlive)
                    component.Owner.RemoveHecsComponent(component);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Owner.World.GlobalUpdateSystem.FinishUpdate -= React;
        }

        public void CommandGlobalReact(RemoveHecsComponentWorldCommand command)
        {
            componentsForRemove.Enqueue(command.Component);
        }
    }
}

namespace Commands
{
    public struct RemoveHecsComponentWorldCommand : IGlobalCommand
    {
        public IComponent Component;
    }
}