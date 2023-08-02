using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.GameLogic, Doc.GameState, "we send this command when we need stop state")]
    public struct StopGameState : IGlobalCommand
	{
		public int GameState;
	}
}