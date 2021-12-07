using Commands;
using HECSFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems
{
    public class WaitingCommandsSystems : BaseSystem, IReactComponent, IReactEntity, IUpdatable, IReactGlobalCommand<WaitAndCallbackCommand>
    {
        private Dictionary<HECSMask, Queue<IWaitingCommand>> waitingCommands = new Dictionary<HECSMask, Queue<IWaitingCommand>>();
        private WaitAndCallbackCommand[] waitAndCallbackCommands = new WaitAndCallbackCommand[128];

        public Guid ListenerGuid { get; } = Guid.NewGuid();

        public void ComponentReact(IComponent component, bool isAdded)
        {
            if (waitingCommands.TryGetValue(component.ComponentsMask, out var globalCommands))
            {
                while (globalCommands.Count > 0)
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
        public void AddWaitingCommand<T>(T command, HECSMask mask) where T : IGlobalCommand
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

        public void UpdateLocal()
        {
            var count = waitAndCallbackCommands.Length;

            for (int i = 0; i < count; i++)
            {
                ref var timer = ref waitAndCallbackCommands[i];

                if (timer.IsOnRun)
                {
                    timer.Timer -= Time.deltaTime;

                    if (timer.Timer <= 0)
                    {
                        timer.IsOnRun = false;
                        timer.CallBack?.Invoke(timer.CallBackWaiter);
                    }
                }
            }
        }

        public void CommandGlobalReact(WaitAndCallbackCommand command)
        {
            Add(ref command);
        }

        private void Add(ref WaitAndCallbackCommand command)
        {
            var count = waitAndCallbackCommands.Length;

            for (int i = 0; i < count; i++)
            {
                ref var timer = ref waitAndCallbackCommands[i];
                if (timer.IsOnRun == false)
                {
                    command.IsOnRun = true;
                    timer = command;
                    return;
                }
            }

            Array.Resize(ref waitAndCallbackCommands, count * 2);
            waitAndCallbackCommands[count] = command;
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