using System;
using System.Buffers;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;

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

        public async UniTask ExecuteActionSequentialy(int Index, Entity to, Entity from = null)
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i].ID == Index)
                {
                    for (int x = 0; x < Actions[i].Actions.Count; x++)
                    {
                        await Actions[i].Actions[x].ActionAsync(to, from);
                    }
                }
            }
        }

        /// <summary>
        /// if different action have same id, we wait all for each action, and run them sequentialy
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public async UniTask ExecuteAction(int Index, Entity to, Entity from = null)
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i].ID == Index)
                {
                    var subPool = ArrayPool<UniTask>.Shared.Rent(Actions[i].Actions.Count);

                    for (int x = 0; x < Actions[i].Actions.Count; x++)
                    {
                        subPool[x] =  Actions[i].Actions[x].ActionAsync(to, from);
                    }

                    await UniTask.WhenAll(subPool);
                    ArrayPool<UniTask>.Shared.Return(subPool, true);
                }
            }
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