using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    /// <summary>
    /// this is abstract base system u should make child of this system for ur project
    /// </summary>
    [Serializable][Documentation(Doc.GameLogic, Doc.GameState, Doc.Global, "this base system what operates states of the game")]
    public abstract class BaseMainGameLogicSystem : BaseSystem, IReactGlobalCommand<EndGameStateCommand>, IGlobalStart 
    {
        [Required]
        public GameStateComponent GameStateComponent;

        public abstract void CommandGlobalReact(EndGameStateCommand command);
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
    }
}