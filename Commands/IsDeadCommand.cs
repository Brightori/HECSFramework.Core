using HECSFramework.Core;

namespace Commands
{
	[Documentation(Doc.Character, "Команда которую мы отправляем локально у актора, что он мертв")]
	public struct IsDeadCommand : ICommand
	{
	}
}