using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HECSFramework.Core
{
    public class Job
    {
        private int threadID;
        private HashSet<IAwaiter> awaiters = new HashSet<IAwaiter>();

        public Job(int thId)
        {
            threadID = thId;
        }

        public void Update()
        {

            Debug.Assert(threadID == Thread.CurrentThread.ManagedThreadId,
                "Was called by a thread that does not own this data");

            awaiters.RemoveWhere((a) => a.TryFinalize());
        }

        internal void AddAwaiter(IAwaiter awaiter)
        {
            awaiters.Add(awaiter);
        } 
    }

    public static class JobsSystem
    {
        private static ConcurrentDictionary<int, Job> jobs = new ConcurrentDictionary<int, Job>();
        public static Job RegisterThreadHandler()
        {
            int thID = Thread.CurrentThread.ManagedThreadId;
            if (jobs.TryAdd(thID, new Job(thID)))
            {
                return jobs[thID];
            }
            throw new Exception("JobsSystem: Attempt to re-register a handler thread");
        }

        internal static void RegisterAwaiter(IAwaiter awaiter)
        {
            int thID = Thread.CurrentThread.ManagedThreadId; 
            if(jobs.TryGetValue(thID, out Job job))
            {
                job.AddAwaiter(awaiter);
            }
            else throw new Exception("JobsSystem: this thread is not registered for awaiter processing");
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
    }
}
