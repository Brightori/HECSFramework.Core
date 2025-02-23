using System;
using System.Collections.Concurrent;

namespace HECSFramework.Core
{
    public class CommandQueueManager<T> : IDisposable where T : struct, IGlobalCommand
    {
        private static HECSList<CommandQueueManager<T>> queueToWorld = new HECSList<CommandQueueManager<T>>(4);
        private World world;

        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public CommandQueueManager(World world)
        {
            this.world = world;
            world.GlobalUpdateSystem.PreFinishUpdate += ProcessQueue;
        }

        private void ProcessQueue()
        {
            var currentCount = queue.Count;

            for (var i = 0; i < currentCount; i++)
            {
                queue.TryDequeue(out var item);
                world.Command(item);
            }
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
                    return queueToWorld[world.Index];
                }
            }

            queueToWorld.AddToIndex(new CommandQueueManager<T>(world), world.Index);
            return queueToWorld[world.Index];
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
