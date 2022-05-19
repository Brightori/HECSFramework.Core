using System.Collections.Generic;
using Commands;
using HECSFramework.Core;

namespace Systems
{
    [Documentation(Doc.GameLogic, "Эта система живет в самом мире, отвечает за то что после всех апдейтов вызовется эта система, и почистит ентити которые мы просим удалить")]
    public sealed class RemoveComponentWorldSystem : BaseSystem, IReactGlobalCommand<RemoveHecsComponentWorldCommand>, IReactGlobalCommand<AddHecsComponentWorldCommand>
    {
        private Queue<IComponent> componentsForRemove = new Queue<IComponent>(8);
        private Queue<AddHecsComponentWorldCommand> componentsToAdd = new Queue<AddHecsComponentWorldCommand>(8);

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

            while (componentsToAdd.Count > 0)
            {
                var command = componentsToAdd.Dequeue();

                if (command.Component != null)
                    command.Entity.AddHecsComponent(command.Component);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            componentsForRemove.Clear();
            componentsToAdd.Clear();
            Owner.World.GlobalUpdateSystem.FinishUpdate -= React;
        }

        public void CommandGlobalReact(RemoveHecsComponentWorldCommand command)
        {
            componentsForRemove.Enqueue(command.Component);
        }

        public void CommandGlobalReact(AddHecsComponentWorldCommand command)
        {
            componentsToAdd.Enqueue(command);
        }
    }
}

namespace Commands
{
    public struct RemoveHecsComponentWorldCommand : IGlobalCommand
    {
        public IComponent Component;
    }

    public struct AddHecsComponentWorldCommand : IGlobalCommand
    {
        public IComponent Component;
        public IEntity Entity;
    }
}