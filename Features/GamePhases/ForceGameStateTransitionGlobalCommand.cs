using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.GameState, "we send this command when we need force jump from one state to another")]
	public struct ForceGameStateTransitionGlobalCommand : IGlobalCommand
	{
		public int GameState;
	}
}