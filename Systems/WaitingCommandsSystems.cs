using System;
using System.Collections.Generic;
using Commands;
using HECSFramework.Core;

namespace Systems
{
    public sealed class WaitingCommandsSystems : BaseSystem, IReactComponent, IReactEntity, IUpdatableDelta, IReactGlobalCommand<WaitAndEntityCallbackCommand>, IReactGlobalCommand<WaitAndCallbackCommand>
    {
        private Dictionary<HECSMask, Queue<IWaitingCommand>> waitingCommands = new Dictionary<HECSMask, Queue<IWaitingCommand>>();

        private ConcurrencyList<WaitAndEntityCallbackCommand> waitAndCallbackEntityCommands = new ConcurrencyList<WaitAndEntityCallbackCommand>(8);
        private Remover<WaitAndEntityCallbackCommand> removerwaitAndCallbackEntityCommands;

        private ConcurrencyList<WaitAndCallbackCommand> waitCallbackCommands = new ConcurrencyList<WaitAndCallbackCommand>(8);
        private Remover<WaitAndCallbackCommand> removerCallbackCommands;

        public Guid ListenerGuid { get; } = Guid.NewGuid();

        public override void InitSystem()
        {
            removerCallbackCommands = new Remover<WaitAndCallbackCommand>(waitCallbackCommands);
            removerwaitAndCallbackEntityCommands = new Remover<WaitAndEntityCallbackCommand>(waitAndCallbackEntityCommands);
        }

        public void ComponentReact<T>(T component, bool isAdded) where T: IComponent
        {
        //todo проверить почему сюда прилетает нал
            if (component == null)
                return;

            if (waitingCommands.TryGetValue(component.ComponentsMask, out var globalCommands))
            {
                while (globalCommands.Count > 0)
                    globalCommands.Dequeue().NowYourTime(Owner.WorldId);
            }
        }

        public void EntityReact(IEntity entity, bool isAdded)
        {
            if (isAdded)
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
        }

        public void AddWaitingCommand<T>(T command, HECSMask mask) where T : struct, IGlobalCommand
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

        public void CommandGlobalReact(WaitAndEntityCallbackCommand command)
        {
            waitAndCallbackEntityCommands.Add(command);
        }

        public void UpdateLocalDelta(float deltaTime)
        {
            removerwaitAndCallbackEntityCommands.ProcessRemoving();
            removerCallbackCommands.ProcessRemoving();
            
            WaitAndCallBackEntityProcess(deltaTime);
            WaitAndCallBackProcess(deltaTime);
        }

        public void WaitAndCallBackEntityProcess(float deltaTime)
        {
            var count = waitAndCallbackEntityCommands.Count;

            for (int i = 0; i < count; i++)
            {
                ref var timer = ref waitAndCallbackEntityCommands.Data[i];

                if (timer.IsOnRun)
                {
                    timer.Timer -= deltaTime;

                    if (timer.Timer <= 0)
                    {
                        timer.IsOnRun = false;
                        timer.CallBack?.Invoke(timer.CallBackWaiter);
                        removerwaitAndCallbackEntityCommands.Add(timer);
                    }
                }
            }
        }

        public void WaitAndCallBackProcess(float deltaTime)
        {
            var count = waitCallbackCommands.Count;

            for (int i = 0; i < count; i++)
            {
                waitCallbackCommands.Data[i].Timer -= deltaTime;

                if (waitCallbackCommands.Data[i].Timer <= 0)
                {
                    waitCallbackCommands.Data[i].CallBack?.Invoke();
                    removerCallbackCommands.Add(waitCallbackCommands.Data[i]);
                }
            }

            removerCallbackCommands.ProcessRemoving();
        }

        public void CommandGlobalReact(WaitAndCallbackCommand command)
        {
            if (command.Commandguid == Guid.Empty)
                command.Commandguid = Guid.NewGuid();

            waitCallbackCommands.Add(command);
        }
    }

    public struct WaitingCommand<T> : IWaitingCommand where T : struct, IGlobalCommand
    {
        private T command;

        public WaitingCommand(T command)
        {
            this.command = command;
        }

        public void NowYourTime(int worldIndex = 0)
        {
            EntityManager.Worlds.Data[worldIndex].Command(command);
        }
    }

    public interface IWaitingCommand
    {
        void NowYourTime(int worldIndex = 0);
    }
}