using HECSFramework.Core;

namespace Commands
{
    public struct DiffCounterCommand<T> : ICommand where T: struct
    {
        public int Id;
        public T Diff;
        public T PreviousValue;
    }
}