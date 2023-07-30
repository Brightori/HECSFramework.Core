using HECSFramework.Core;

namespace Commands
{
	[Documentation(Doc.GameState, Doc.GameLogic, "we send this command with context of switching phase")]
	public struct TransitionGameStateCommand : IGlobalCommand
	{
		//game state identifier
		public int From;
		public int To;
	}
}