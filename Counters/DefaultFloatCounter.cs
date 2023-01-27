using System;

namespace HECSFramework.Core
{
    [Serializable]
    public class DefaultFloatCounter : ICounter<float>
    {
        public float Value { get; private set; }

        public int Id { get; set; }

        public void ChangeValue(float value)
        {
            Value += value;
        }

        public void SetValue(float value)
        {
            Value = value;
        }
    }

    [Serializable]
    public class DefaultIntCounter : ICounter<int>
    {
        public int Value { get; private set; }
        public int Id { get; set; }

        public void ChangeValue(int value)
        {
            Value += value;
        }

        public void SetValue(int value)
        {
            Value = value;
        }
    }
}