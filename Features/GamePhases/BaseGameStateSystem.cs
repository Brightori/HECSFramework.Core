using Commands;
using HECSFramework.Core;

namespace Systems
{
    /// <summary>
    /// its just helper system for fast integration to game states systems
    /// </summary>
    [Documentation(Doc.GameState, Doc.GameLogic, "this system participate at the game loop with TransitionGameStateCommand")]
    public abstract class BaseGameStateSystem : BaseSystem, IReactGlobalCommand<TransitionGameStateCommand>
    {
        public void CommandGlobalReact(TransitionGameStateCommand command)
        {
            ProcessState(command.From, command.To);
        }

        protected abstract void ProcessState(int from, int to);
    }
}