using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    public interface ICommandQueue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ProcessQueue();
    }

    public class CommandQueueManager<T> : ICommandQueue, IDisposable where T : struct, IGlobalCommand
    {
        private static HECSList<CommandQueueManager<T>> queueToWorld = new HECSList<CommandQueueManager<T>>(4);
        private World world;

        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public CommandQueueManager(World world)
        {
            this.world = world;
        }

        public static void AddToQueue(World world, T command)
        {
            if (!world.IsAlive)
            {
                HECSDebug.LogError("[CommandQueueManager] we try send command to dead world");
                return;
            }

            var neededQueueManager = GetCommandQueueManager(world);
            neededQueueManager.AddToLocalQueue(command);
            world.AddToProccessQueueCommand(neededQueueManager);
        }

        /// <summary>
        /// we use default world in this realisation its more usable for client|standalone application
        /// </summary>
        /// <param name="command"></param>
        public static void AddToQueue(T command)
        {
            if (!EntityManager.Default.IsAlive)
            {
                HECSDebug.LogError("[CommandQueueManager] we try send command to dead world");
                return;
            }

            var neededQueueManager = GetCommandQueueManager(EntityManager.Default);
            neededQueueManager.AddToLocalQueue(command);
            EntityManager.Default.AddToProccessQueueCommand(neededQueueManager);
        }

        public void AddToLocalQueue(T command)
        {
            queue.Enqueue(command);
        }

        private static CommandQueueManager<T> GetCommandQueueManager(World world)
        {
            if (queueToWorld.Count > world.Index)
            {
                if (queueToWorld[world.Index] != null)
                    return queueToWorld[world.Index];
                else
                {
                    queueToWorld.AddToIndex(new CommandQueueManager<T>(world), world.Index);

                    world.OnWorldDispose += queueToWorld[world.Index].Dispose;
                    return queueToWorld[world.Index];
                }
            }

            queueToWorld.AddToIndex(new CommandQueueManager<T>(world), world.Index);
            world.OnWorldDispose += queueToWorld[world.Index].Dispose;
            return queueToWorld[world.Index];
        }

        public void Dispose()
        {
            queueToWorld[world.Index] = null;
            world.OnWorldDispose -= Dispose;
            queue.Clear();
            world = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICommandQueue.ProcessQueue()
        {
            if (queue.TryDequeue(out var item))
                world.Command(item);
        }
    }
}