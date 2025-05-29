using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Feature("Execute Actions Helper sys")]
    [Documentation(Doc.Action, Doc.HECS, "this system execute actions by action helper, u can use this if u have more then one actions holder on entity and want use commands")]
    public sealed class ExecuteActionsHelperSystem : BaseSystem, IReactCommand<ExecuteActionCommand>
    {
        [Required]
        public ActionsExecuteHelperComponent ActionsExecuteHelperComponent;

        public void CommandReact(ExecuteActionCommand command)
        {
            ActionsExecuteHelperComponent.ExecuteAction(command.ActionID, command.Target);
        }

        public override void InitSystem()
        {
        }
    }
}

namespace Commands
{
    public struct ExecuteActionCommand : ICommand
    {
        public int ActionID;
        public Entity Target;
    }
}
