using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HECSFramework.Core
{
    public class ExecuteInUpdate : IUpdatable, IHECSJobRunPooler
    {
        private ConcurrentQueue<ActionInUpdate> actions = new();
        private Dictionary<Type, Stack<IJobProcessor>> pooling = new Dictionary<Type, Stack<IJobProcessor>>(32);

        private ConcurrencyList<IJobProcessor> jobProcessors = new ConcurrencyList<IJobProcessor>(16);
        private AddAndRemoveHelper<IJobProcessor> addAndRemoveHelper;

        public ExecuteInUpdate()
        {
            addAndRemoveHelper = new AddAndRemoveHelper<IJobProcessor>(jobProcessors);
        }

        public async ValueTask ExecuteAction(Action action)
        {
            var newAction = new ActionInUpdate(action);
             actions.Enqueue(newAction);

            while (!newAction.IsComplete)
            {
                await Task.Delay(1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<T> RunJob<T>(T job) where T : struct, IHecsJob
        {
            var result = await ExecuteJob(job);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HECSJobRun<T> ExecuteJob<T>(T job) where T: struct, IHecsJob
        {
            var key = typeof(T);

            if (pooling.TryGetValue(key, out var stack))
            {
                if (stack.TryPop(out var processor))
                {
                    var jobRun = (processor as HECSJobRun<T>);
                    jobRun.Job = job;
                    addAndRemoveHelper.Add(processor);
                    return jobRun;
                }
            }

            var newJobRun = new HECSJobRun<T>(this);
            newJobRun.Job = job;
            addAndRemoveHelper.Add(newJobRun);
            return newJobRun;
        }

        public void Release(IJobProcessor jobProcessor)
        {
            if (pooling.TryGetValue(jobProcessor.Key, out var stack))
                stack.Push(jobProcessor);
            else
            {
                var newStack = new Stack<IJobProcessor>();
                newStack.Push(jobProcessor);
                pooling.Add(jobProcessor.Key, newStack);
            }
        }

        public void UpdateLocal()
        {
            while (actions.TryDequeue(out var action))
                action.UpdateLocal();

            addAndRemoveHelper.Process();
            var count = jobProcessors.Count;
            
            for (int i = 0; i < count; i++)
            {
                jobProcessors.Data[i].Update();
            }
            addAndRemoveHelper.Process();
        }
    }

    public interface IHECSJobRunPooler
    {
        void Release(IJobProcessor jobProcessor);
    }

    public class ActionInUpdate : IUpdatable
    {
        private Action action;
        public bool IsComplete;

        public ActionInUpdate(Action action, bool isComplete = false) 
        {
            this.action = action;
            IsComplete = isComplete;
        }

        public void UpdateLocal()
        {
            action?.Invoke();
            IsComplete = true;
        }
    }

    public interface IHecsJob
    {
        void Run();
        bool IsComplete();
    }

    public sealed class HECSJobRun<T> : IJobProcessor, INotifyCompletion where T : struct, IHecsJob
    {
        public T Job;
        private bool abortOperation;

        public Type Key { get; }
        public bool IsCompleted => Job.IsComplete();

        private IHECSJobRunPooler jobRunPooler;
        private event Action onComplete;

        public HECSJobRun(IHECSJobRunPooler jobRunPooler)
        {
            this.jobRunPooler = jobRunPooler;
            Key = typeof(T);
        }

        public void AbortOperation()
        {
            abortOperation = true;
        }

        public void Update()
        {
            Job.Run();

            if (Job.IsComplete() || abortOperation)
            {
                onComplete?.Invoke();
                Release();
            }
        }

        public void Release()
        {
            onComplete = null;
            jobRunPooler.Release(this);
        }

        public void OnCompleted(Action continuation)
        {
            onComplete += continuation;
        }

        public HECSJobRun<T> GetAwaiter() 
        {
            return this;
        }

        public T GetResult()
        {
            if (!IsCompleted)
                throw new Exception("we dont complete job");

            return Job;
        }
    }

    public interface IJobProcessor
    {
        Type Key { get; }
        void Update();
        void Release();
    }
}
