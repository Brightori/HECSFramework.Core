using HECSFramework.Core;


namespace Components
{
    public abstract class SimpleIntCounterBaseComponent : BaseComponent, ICounter<int>
    {
        public abstract  int Value { get; protected set; }
        public abstract int Id { get; }

        public virtual void ChangeValue(int value)
        {
            Value += value;
        }

        public virtual void SetValue(int value)
        {
            Value = value;
        }
    }
}
