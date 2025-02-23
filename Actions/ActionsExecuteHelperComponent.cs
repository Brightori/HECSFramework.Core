using System.Linq;
using HECSFramework.Core;

namespace Components
{
    public sealed class ActionsExecuteHelperComponent : BaseComponent
    {
        private IActionsHolderComponent[] actionsHolders;

        public override void AfterInit()
        {
            actionsHolders = Owner.GetComponentsByType<IActionsHolderComponent>().ToArray();
        }

        public void ExecuteAction(int id, Entity owner)
        {
            foreach (var action in actionsHolders)
            {
                action.ExecuteAction(id, owner);
            }
        }
    }
}