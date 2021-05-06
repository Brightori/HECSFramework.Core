using HECSFramework.Core;
using System;
using System.Collections.Generic;

namespace Systems
{
    public class WaitingCommandsSystems : BaseSystem, IReactComponent, IReactEntity
    {
        private Dictionary<HECSMask, Queue<IWaitingCommand>> waitingCommands = new Dictionary<HECSMask, Queue<IWaitingCommand>>();

        public Guid ListenerGuid { get; } = Guid.NewGuid();

        public void ComponentReact(IComponent component, bool isAdded)
        {
            if (waitingCommands.TryGetValue(component.ComponentsMask, out var globalCommands))
            {
                while(globalCommands.Count > 0)
                    globalCommands.Dequeue().NowYourTime(Owner.WorldId);
            }
        }

        public void EntityReact(IEntity entity, bool isAdded)
        {
            foreach (var kv in waitingCommands)
            {
                var key = kv.Key;
                if (entity.ContainsMask(ref key))
                {
                    while (kv.Value.Count > 0)
                    {
                        kv.Value.Dequeue().NowYourTime(Owner.WorldId);
                    }
                }
            }
        }

        public void AddWaitingCommand<T>(T command, HECSMask mask) where T: IGlobalCommand
        {
            if (waitingCommands.TryGetValue(mask, out var globalCommands))
                globalCommands.Enqueue(new WaitingCommand<T>(command));
            else
            {
                var newQueue = new Queue<IWaitingCommand>();
                newQueue.Enqueue(new WaitingCommand<T>(command));
                waitingCommands.Add(mask, newQueue);
            }
        }

        public override void InitSystem()
        {
        }
    }

    public struct WaitingCommand<T> : IWaitingCommand where T : IGlobalCommand
    {
        private T command;

        public WaitingCommand(T command)
        {
            this.command = command;
        }

        public void NowYourTime(int worldIndex = 0)
        {
            EntityManager.Worlds[worldIndex].Command(command);
        }
    }

    public interface IWaitingCommand
    {
        void NowYourTime(int worldIndex = 0);
    }
}