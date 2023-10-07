using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.GameState, "this component hold index of current game state, we use it at game state system")]
    public sealed partial class GameStateComponent : BaseComponent, IWorldSingleComponent
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

        public bool IsNeededState(int index1)
        {
            return (CurrentState == index1);
        }

        public bool IsNeededState(int index1, int index2) 
        { 
            return IsNeededState(index1) || IsNeededState(index2);
        }

        public bool IsNeededState(int index1, int index2, int index3)
        {
            return IsNeededState(index1, index2) || IsNeededState(index3);
        }

        public bool IsNeededState(int index1, int index2, int index3, int index4)
        {
            return IsNeededState(index1, index2) || IsNeededState(index3, index4);
        }
    }
}