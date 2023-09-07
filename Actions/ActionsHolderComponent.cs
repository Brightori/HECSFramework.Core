using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Abilities, "this component holds actions with identifier, we can call visual actions or execite composite actions by this component")]
    public sealed partial class ActionsHolderComponent : BaseActionsHolderComponent
    {
    }

    public abstract partial class BaseActionsHolderComponent : BaseComponent, IDisposable, IActionsHolderComponent
    {
        protected List<ActionsToIdentifier> Actions = new List<ActionsToIdentifier>(4);

        public void ExecuteAction(int Index, Entity entity = null)
        {
            if (entity == null)
                entity = Owner;

            foreach (var a in Actions)
            {
                if (a.ID == Index)
                {
                    foreach (var action in a.Actions)
                        action.Action(entity);
                }
            }
        }

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
    }

    public interface IActionsHolderComponent
    {
        void ExecuteAction(int Index, Entity entity = null);
    }

    public struct ActionsToIdentifier
    {
        public int ID;
        public List<IAction> Actions;
    }
}