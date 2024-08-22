using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using Helpers;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Abilities, "this component holds actions with identifier, we can call visual actions or execite composite actions by this component")]
    public sealed partial class AsyncActionsHolderComponent : AsyncBaseActionsHolderComponent
    {
    }

    public abstract partial class AsyncBaseActionsHolderComponent : BaseComponent, IDisposable, IAsyncActionsHolderComponent
    {
        protected HECSList<AsyncActionsToIdentifier> Actions = new HECSList<AsyncActionsToIdentifier>(4);
        private Stack<HECSList<UniTask>> readyForPool = new Stack<HECSList<UniTask>>();
        private HECSList<(int index, HECSList<UniTask> uniTasks)> inProgress = new HECSList<(int index, HECSList<UniTask> uniTasks)>(2);
        private int index;

        public void Dispose()
        {
            foreach (var a in Actions)
            {
                foreach (var action in a.Actions)
                {
                    if (action is IDisposable disposable)
                        disposable.Dispose();
                }
            }
        }

        public async UniTask ExecuteAction(int Index, Entity to, Entity from = null)
        {
            index++;

            var pool = GetList(index);

            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i].ID == Index)
                {
                    var subPool = GetList(index);

                    foreach (var a in Actions[i].Actions)
                    {
                        subPool.Add(a.ActionAsync(to, from));
                    }

                    pool.Add(UniTask.WhenAll(subPool));
                }
            }

            if (pool.Count > 0)
                await UniTask.WhenAll(pool);

            Release(index);
        }

        public void Release(int index)
        {
            var pool = HECSPooledArray<(int index, HECSList<UniTask> uniTasks)>.GetArray(inProgress.Count);

            foreach (var kp in inProgress)
            {
                if (kp.index == index)
                {
                    pool.Add(kp);
                    readyForPool.Push(kp.uniTasks);
                    kp.uniTasks.ClearFast();
                }
            }

            for (int i = 0; i < pool.Count; i++)
            {
                inProgress.RemoveSwap(pool.Items[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HECSList<UniTask> GetList(int currentIndex)
        {
            if (readyForPool.TryPop(out var list))
            {
                inProgress.Add((currentIndex, list));
                return list;
            }

            var newList = new HECSList<UniTask>(2);
            inProgress.Add((currentIndex, newList));
            return newList;
        }
    }

    public interface IAsyncActionsHolderComponent
    {
        UniTask ExecuteAction(int Index, Entity to, Entity from = null);
    }

    public struct AsyncActionsToIdentifier
    {
        public int ID;
        public List<IAsyncAction> Actions;
    }
}