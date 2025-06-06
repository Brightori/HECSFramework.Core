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

        public DefaultFloatCounter()
        {
        }

        public DefaultFloatCounter(int id)
        {
            this.id = id;
        }

        public DefaultFloatCounter(float value, int id)
        {
            this.value = value;
            this.id = id;
        }

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

        public DefaultIntCounter()
        {
        }

        public DefaultIntCounter(int id)
        {
            this.id = id;
        }

        public DefaultIntCounter(int value, int id)
        {
            this.value = value;
            this.id = id;
        }

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