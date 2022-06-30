using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Counters, "This command represent diff after modifying counter")]
    public struct DiffCounterCommand<T> : ICommand where T: struct
    {
        public int Id;
        public T Value;
        public T PreviousValue;
        public T MaxValue;
    }
}