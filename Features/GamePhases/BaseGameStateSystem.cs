using System.Collections.Generic;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    /// <summary>
    /// its just helper system for fast integration to game states systems
    /// </summary>
    [Documentation(Doc.GameState, Doc.GameLogic, "this system participate at the game loop with TransitionGameStateCommand")]
    public abstract class BaseGameStateSystem : BaseSystem, IReactGlobalCommand<TransitionGameStateCommand>, IReactGlobalCommand<StopGameStateGlobalCommand>
    {
        protected abstract int State { get; }

        //we lock transition to this state only by this states, if this count > 0,
        //from should be contained in this hashset for valid transition
        private HashSet<int> lockedStates = new HashSet<int>(0);

        //we can have various force transitions from this state
        private HECSList<IForceTransition> forceTransitions = new HECSList<IForceTransition>(2);

        public void CommandGlobalReact(TransitionGameStateCommand command)
        {
            if (command.To != State)
                return;

            if (lockedStates.Count > 0 && !lockedStates.Contains(command.From))
            {
#if IdentifiersGenerated
                HECSDebug.LogWarning($"we try to enter this state {IdentifierToStringMap.IntToString[State]}, from not valid state {IdentifierToStringMap.IntToString[command.From]}");
#endif
                return;
            }

            ProcessState(command.From, command.To);
        }

        protected void AddLockFromState(int stateIndex)
        {
            lockedStates.Add(stateIndex);
        }

        protected void AddForceTransitions(IForceTransition forceTransition)
        {
            forceTransitions.Add(forceTransition);
        }

        protected abstract void ProcessState(int from, int to);

        /// <summary>
        /// shortcut for end a state
        /// </summary>
        protected void EndState()
        {
            OnExitState();

            foreach (var transition in forceTransitions) 
            {
                if (transition.ForceTransitionComplete(Owner, State))
                    return;
            }

            Owner.World.Command(new EndGameStateCommand(State));
        }

        protected virtual void OnExitState()
        {
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

        protected bool IsNeededStates(int from, int to)
        {
            var state = Owner.World.GetSingleComponent<GameStateComponent>();

            return from == state.PreviousState && to == state.CurrentState;
        }

        public void CommandGlobalReact(StopGameStateGlobalCommand command)
        {
            if (command.GameState == State)
            {
                OnExitState();
                StopState();
            }
        }

        /// <summary>
        /// u should override at child this method, for implementation of stoping state
        /// </summary>
        protected virtual void StopState()
        {
        }
    }

    public interface IForceTransition
    {
        bool ForceTransitionComplete(Entity owner, int CurrentState);
    }
}