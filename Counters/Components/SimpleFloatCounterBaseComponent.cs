using HECSFramework.Core;


namespace Components
{
    public abstract class SimpleFloatCounterBaseComponent : BaseComponent, ICounter<float>
    {
        public abstract float Value { get; protected set; }
        public abstract int Id { get; }

        public void ChangeValue(float value)
        {
            Value += value;
        }

        public void SetValue(float value)
        {
            Value = value;
        }
    }
}
