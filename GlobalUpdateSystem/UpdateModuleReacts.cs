using System;
using System.Collections.Generic;
using HECSFramework.Core.Helpers;

namespace HECSFramework.Core
{
    public class UpdateModuleReacts : 
        IRegisterUpdate<DelayedAction>, 
        IRegisterUpdate<AddUpdateWithPredicate>, 
        IRegisterUpdate<DispatchGlobalCommand>
    {
        private readonly Queue<Action> queueFromAsync = new Queue<Action>();
        private readonly HashSet<Func<bool>> updateFuncs = new HashSet<Func<bool>>();
        private readonly List<DelayedAction> delayedActions = new List<DelayedAction>(16);
        private bool funcResult;
        private static float currentDeltaTime;

        public void UpdateLocal(float deltaTime)
        {
            currentDeltaTime = deltaTime;

            UpdateDelayedActions();
            OneWayQueue();
            UpdateGlobalFunc();
        }

        private void UpdateDelayedActions()
        {
            if (delayedActions.Count == 0)
                return;

            for (int i = 0; i < delayedActions.Count; i++)
            {
                var index = i;
                var timer = delayedActions[index];
                
                UpdateTimer(ref timer.TimeToRun);

                if (timer.TimeToRun <= 0)
                {
                    delayedActions[index].Action?.Invoke();
                    delayedActions.RemoveAt(index);
                    continue;
                }

                delayedActions[index] = new DelayedAction { Action = timer.Action, TimeToRun = timer.TimeToRun };
            }
        }

        private static void UpdateTimer(ref float timer)
        {
            timer -= currentDeltaTime;
        }
        
        private void OneWayQueue()
        {
            if (queueFromAsync.Count == 0)
                return;

            while (queueFromAsync.Count > 0)
            {
                var act = queueFromAsync.Dequeue();
                act.Invoke();
            }
        }

        private void UpdateGlobalFunc()
        {
            if (updateFuncs.Count == 0)
                return;

            foreach (var f in updateFuncs)
            {
                funcResult = f();

                if (funcResult)
                    queueFromAsync.Enqueue(() => updateFuncs.Remove(f));
            }
        }

        public void Register(DelayedAction updatable, bool add)
        {
            delayedActions.AddOrRemoveElement(updatable, add);
        }

        public void Register(AddUpdateWithPredicate updatable, bool add)
        {
            updateFuncs.Add(updatable.Func);
        }

        public void Register(DispatchGlobalCommand updatable, bool add)
        {
            queueFromAsync.Enqueue(updatable.Action);
        }
    }
    
    public struct DelayedAction : IRegisterUpdatable
    {
        public float TimeToRun;
        public Action Action { get; set; }
    }
    public struct DispatchGlobalCommand : IRegisterUpdatable
    {
        public Action Action;
    }

    public struct AddUpdateWithPredicate : IRegisterUpdatable
    {
        public Func<bool> Func;
    }
}