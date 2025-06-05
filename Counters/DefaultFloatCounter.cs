using System;
using Helpers;

namespace HECSFramework.Core
{
    [Serializable]
    public partial class DefaultFloatCounter : ICounter<float>
    {
        private float value;

        [IdentifierDropDown("CounterIdentifierContainer")]
        private int id;

        public float Value => value;
        public int Id => id;

        public void ChangeValue(float value)
        {
            this.value += value;
        }

        public void SetValue(float value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public partial class DefaultIntCounter : ICounter<int>
    {
        private int value;

        [IdentifierDropDown("CounterIdentifierContainer")]
        private int id;

        public int Value => value;
        public int Id => id;

        public void ChangeValue(int value)
        {
            this.value += value;
        }

        public void SetValue(int value)
        {
            this.value = value;
        }
    }
}