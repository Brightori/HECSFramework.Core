using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Abilities, "this component holds actions with identifier, we can call visual actions or execite composite actions by this component")]
    public sealed partial class ActionsHolderComponent : BaseComponent
    {
        private List<ActionsToIdentifier> Actions = new List<ActionsToIdentifier>(4);

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
    }

    public struct ActionsToIdentifier
    {
        public int ID;
        public List<IAction> Actions;
    }
}