using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HECSFramework.Core
{
    public class ExecuteInUpdate : IUpdatable
    {
        private ConcurrentQueue<ActionInUpdate> actions = new();

        public async ValueTask ExecuteAction(Action action)
        {
            var newAction = new ActionInUpdate(action);
             actions.Enqueue(newAction);

            while (!newAction.IsComplete)
            {
                await Task.Delay(1);
            }
        }

        public void UpdateLocal()
        {
            while (actions.TryDequeue(out var action))
                action.UpdateLocal();
        }
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
}
