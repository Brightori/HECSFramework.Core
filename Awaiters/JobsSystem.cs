using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
namespace HECSFramework.Core
{
    public class AwaiterProcessor
    {
        private int threadID;
        private HECSList<IAwaiter> awaiters = new();
        private Queue<IAwaiter> removeQueue = new();

        public AwaiterProcessor(int thId)
        {
            threadID = thId;
        }

        public void Update()
        {
            Debug.Assert(threadID == Thread.CurrentThread.ManagedThreadId,
                "Was called by a thread that does not own this data");

            for (int i = 0; i < awaiters.Count; i++)
            {
                if (awaiters.Data[i].TryFinalize())
                    removeQueue.Enqueue(awaiters.Data[i]);
            }

            while (removeQueue.TryDequeue(out var awaiter))
            {
                awaiters.RemoveSwap(awaiter);
            }
        }

        internal void AddAwaiter(IAwaiter awaiter)
        {
            awaiters.Add(awaiter);
        }
    }

    public static class AwaitersService
    {
        private static ConcurrentDictionary<int, AwaiterProcessor> jobs = new ConcurrentDictionary<int, AwaiterProcessor>();
        
        public static AwaiterProcessor RegisterThreadHandler()
        {
            int thID = Thread.CurrentThread.ManagedThreadId;
            return jobs.GetOrAdd(thID, new AwaiterProcessor(thID));
        }

        internal static void RegisterAwaiter(IAwaiter awaiter)
        {
            int thID = Thread.CurrentThread.ManagedThreadId;

            if (jobs.TryGetValue(thID, out AwaiterProcessor job))
                job.AddAwaiter(awaiter);
            else
                throw new InvalidOperationException("we dont have awaiter processor for this thread" + thID);
        }

        public static async Awaiter<T> Execute<T>(Task<T> task)
        {
            await task;
            return task.Result;
        }

        public static async Awaiter Execute(Task task)
        {
            await task;
        }

        public static async Awaiter Execute(YieldAwaitable task)
        {
            await task;
        }
    }
}