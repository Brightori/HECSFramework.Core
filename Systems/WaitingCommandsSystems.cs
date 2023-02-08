using System;
using System.Collections.Generic;
using Commands;
using HECSFramework.Core;

namespace Systems
{
    public sealed class WaitingCommandsSystems : BaseSystem, IUpdatableDelta, IReactGlobalCommand<WaitAndEntityCallbackCommand>, IReactGlobalCommand<WaitAndCallbackCommand>
    {
        private HECSList<WaitAndEntityCallbackCommand> waitAndCallbackEntityCommands = new HECSList<WaitAndEntityCallbackCommand>(8);
        private Remover<WaitAndEntityCallbackCommand> removerwaitAndCallbackEntityCommands;

        private HECSList<WaitAndCallbackCommand> waitCallbackCommands = new HECSList<WaitAndCallbackCommand>(8);
        private Remover<WaitAndCallbackCommand> removerCallbackCommands;

        public Guid ListenerGuid { get; } = Guid.NewGuid();

        public override void InitSystem()
        {
            removerCallbackCommands = new Remover<WaitAndCallbackCommand>(waitCallbackCommands);
            removerwaitAndCallbackEntityCommands = new Remover<WaitAndEntityCallbackCommand>(waitAndCallbackEntityCommands);
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
            EntityManager.Worlds[worldIndex].Command(command);
        }
    }

    public interface IWaitingCommand
    {
        void NowYourTime(int worldIndex = 0);
    }
}