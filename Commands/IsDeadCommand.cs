using HECSFramework.Core;

namespace Commands
{
	[Documentation(Doc.Character, "we send this command from actor|entity, from health system when they die, have death event")]
	public struct IsDeadCommand : ICommand
	{
	}
}