using Components;
using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.GameLogic, Doc.GameState, "This command is designed to stop the current state externally")]
    public struct ForceEndCurrentGameStateCommand : IGlobalCommand
    {
        public int GameState;

        public ForceEndCurrentGameStateCommand(World world)
        {
            GameState = world.GetSingleComponent<GameStateComponent>().CurrentState;
        }
    }
}