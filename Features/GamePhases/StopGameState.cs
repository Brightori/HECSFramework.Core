using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.GameLogic, Doc.GameState, "we send this command when we need stop state")]
    public struct StopGameStateGlobalCommand : IGlobalCommand
	{
		public int GameState;

		public StopGameStateGlobalCommand(int gameState)
		{
			GameState = gameState;
		}
	}
}