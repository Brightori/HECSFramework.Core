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

        protected void EndState()
        {
            Owner.World.Command(new EndGameStateCommand());
        }

        /// <summary>
        /// helper method for cheking needed states
        /// </summary>
        /// <param name="from">we provide here information from transition command</param>
        /// <param name="fromCheck">needed from state (GameStateIdentifierMap)</param>
        /// <param name="to">we provide here information from transition command (to)</param>
        /// <param name="toCheck">needed (to) from state (GameStateIdentifierMap)</param>
        /// <returns></returns>
        protected bool IsNeededStates(int from, int fromCheck, int to, int toCheck)
        {
            return from == fromCheck && to == toCheck;
        }
    }
}