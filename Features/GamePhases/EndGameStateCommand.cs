using HECSFramework.Core;

namespace Commands
{
	[Documentation(Doc.GameLogic, Doc.GameState, "we send this command from state when state complete, u should provide index of state")]
    public struct EndGameStateCommand : IGlobalCommand
	{
		public int GameState;

		public EndGameStateCommand(int gameState)
		{
			GameState = gameState;
		}
	}
}