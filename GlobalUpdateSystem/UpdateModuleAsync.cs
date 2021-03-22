using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HECSFramework.Core 
{
    public class UpdateModuleAsync : IDisposable, IRegisterUpdate<IAsyncUpdatable>
    {
        private List<TaskContainer> taskContainers = new List<TaskContainer>(8);

        public void Dispose()
        {
            foreach (var taskContainer in taskContainers)
                taskContainer.CancellationTokenSource.Cancel();
        }

        public void Register(IAsyncUpdatable updatable, bool add)
        {
            var neededUpdate = taskContainers.FirstOrDefault(x => x.TimeDelay == updatable.IntervalInMilliseconds);

            if (add)
            {
                if (neededUpdate == null)
                {
                    var newUpdate = new TaskContainer(updatable.IntervalInMilliseconds);
                    taskContainers.Add(newUpdate);
                    newUpdate.AddToUpdate(updatable);
                }
                else
                {
                    neededUpdate.AddToUpdate(updatable);
                }
            }
            else
            {
                if (neededUpdate == null)
                    return;

                neededUpdate.RemoveFromUpdate(updatable);
            }
        }
    }

    public class TaskContainer
    {
        public CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private CancellationToken cancellationToken = new CancellationToken();
        public int TimeDelay { get; private set; } = 20;

        public List<IAsyncUpdatable> updatableAsyncs = new List<IAsyncUpdatable>(16);
        private Queue<IAsyncUpdatable> addQueue = new Queue<IAsyncUpdatable>(16);
        private Queue<IAsyncUpdatable> removeQueue = new Queue<IAsyncUpdatable>(16);

        private bool isInited;

        public TaskContainer()
        {
            RunAsyncUpdate();   
        }

        public TaskContainer(int ms)
        {
            TimeDelay = ms;
            RunAsyncUpdate();
        }

        private void RunAsyncUpdate()
        {
            if (isInited)
                return;

            cancellationToken = CancellationTokenSource.Token;

            Task.Factory.StartNew(UpdateAsync, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            isInited = true;
        }

        public void AddToUpdate(IAsyncUpdatable updatableAsync)
        {
            if (updatableAsyncs.Contains(updatableAsync))
                return;

            addQueue.Enqueue(updatableAsync);
        }

        public void RemoveFromUpdate(IAsyncUpdatable updatableAsync)
        {
            if (!updatableAsyncs.Contains(updatableAsync))
                return;

            removeQueue.Enqueue(updatableAsync);
        }

        public async Task UpdateAsync()
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                while (addQueue.Count > 0)
                {
                    var needToAdd = addQueue.Dequeue();

                    if (updatableAsyncs.Contains(needToAdd) || needToAdd == null || !needToAdd.Owner.IsAlive)
                        continue;

                    updatableAsyncs.Add(needToAdd);
                }

                while (removeQueue.Count > 0)
                {
                    var needToRemove = removeQueue.Dequeue();

                    if (updatableAsyncs.Contains(needToRemove))
                        updatableAsyncs.Remove(needToRemove);
                }

                var count = updatableAsyncs.Count;

                for (int i = 0; i < count; i++)
                {
                    if (updatableAsyncs[i].Owner.IsAlive)
                        updatableAsyncs[i].UpdateAsyncLocal();
                }


                if (cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(TimeDelay);
            }
        }
    }

    public interface IAsyncUpdatable : IHaveOwner, IRegisterUpdatable
    {
        int IntervalInMilliseconds { get; }
        void UpdateAsyncLocal();
    }
}