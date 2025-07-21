using Commands;
using HECSFramework.Core;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Counters, "Simple int counter component with sending command on change value")]
    public abstract class SimpleIntCounterWithDiffBaseComponent : BaseComponent, ICounter<int>
    {
        public abstract int Value { get; protected set; }
        public abstract int Id { get; }

        public void ChangeValue(int value)
        {
            Owner.Command(new DiffCounterCommand<int>()
            {
                Id = Id,
                PreviousValue = Value,
                Value = Value + value
            });
            Value += value;
        }

        public void SetValue(int value)
        {
            Value = value;
        }
    }
}