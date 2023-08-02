using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.GameState, "this component hold index of current game state, we use it at game state system")]
    public sealed class GameStateComponent : BaseComponent, IWorldSingleComponent
    {
        public int CurrentState { get; private set; }
        public int PreviousState { get; private set; }

        /// <summary>
        /// here we provide game state identifier
        /// </summary>
        /// <param name="index">game state identifier</param>
        public void SetState(int index)
        {
            PreviousState = CurrentState;
            CurrentState = index;
        }
    }
}