using System;
using System.Collections.Generic;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    /// <summary>
    /// this is abstract base system u should make child of this system for ur project
    /// </summary>
    [Serializable][Documentation(Doc.GameLogic, Doc.GameState, Doc.Global, "this base system what operates states of the game")]
    public abstract class BaseMainGameLogicSystem : BaseSystem, IReactGlobalCommand<EndGameStateCommand>, IReactGlobalCommand<ForceGameStateTransitionGlobalCommand>, IGlobalStart, IPriorityUpdatable 
    {
        [Required]
        public GameStateComponent GameStateComponent;

        private Queue<EndGameStateCommand> endGameStateCommands = new Queue<EndGameStateCommand>(2);
        private Queue<ForceGameStateTransitionGlobalCommand> forceStateCommands = new Queue<ForceGameStateTransitionGlobalCommand>(2);

        public int Priority { get; } = -1;

        protected abstract void ProcessEndState(EndGameStateCommand endGameStateCommand);

        public void CommandGlobalReact(EndGameStateCommand command)
        {
            if (command.GameState != GameStateComponent.CurrentState)
                return;

            endGameStateCommands.Enqueue(command);
        }

        public abstract void GlobalStart();

        protected void ChangeGameState(int from, int to)
        {
            GameStateComponent.SetState(to);
            Owner.World.Command(new TransitionGameStateCommand { From = from, To = to });
        }

        protected void ChangeGameState(int to)
        {
            var from = GameStateComponent.CurrentState;
            ChangeGameState(from, to);
        }

        public void CommandGlobalReact(ForceGameStateTransitionGlobalCommand command)
        {
            forceStateCommands.Enqueue(command);
        }

        protected virtual void ProcessForceState(ForceGameStateTransitionGlobalCommand command)
        {
            Owner.World.Command(new StopGameStateGlobalCommand(GameStateComponent.CurrentState));
            ChangeGameState(GameStateComponent.CurrentState, command.GameState);
        }

        public void PriorityUpdateLocal()
        {
            if (endGameStateCommands.TryDequeue(out EndGameStateCommand command))
                ProcessEndState(command);

            if (forceStateCommands.TryDequeue(out ForceGameStateTransitionGlobalCommand forceCommand))
                ProcessForceState(forceCommand);
        }
    }
}