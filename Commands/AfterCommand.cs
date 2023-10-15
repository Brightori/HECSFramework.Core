using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.Helpers, Doc.Global, "we use this command when we need send universal context or build chain of commands, we can send inside this command other command")]
    public struct AfterCommand<T> : IGlobalCommand
    {
        public T Value;
    }
}
