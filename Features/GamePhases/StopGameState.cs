using HECSFramework.Core;

namespace Commands
{
	/// <summary>
	/// its service command
	/// if u want stop state manualy, u should use end state command instead of this
	/// </summary>
    [Documentation(Doc.GameLogic, Doc.GameState, "we send this command when we need stop state, this is service command, u dont need send it manualy, we send it from main game state system")]
    public struct StopGameStateGlobalCommand : IGlobalCommand
	{
		public int GameState;

		public StopGameStateGlobalCommand(int gameState)
		{
			GameState = gameState;
		}
	}
}