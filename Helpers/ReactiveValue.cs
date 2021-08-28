using System;
using System.Collections.Generic;

namespace HECSFramework.Core.Helpers
{
    public class ReactiveValue<T> : IDisposable
    {
        private T currentState;

        private readonly EqualityComparer<T> comparer;
        public event Action<T> OnChange;

        public ReactiveValue()
        {
            comparer = EqualityComparer<T>.Default;
        }

        public ReactiveValue(T defaultValue)
        {
            currentState = defaultValue;
            comparer = EqualityComparer<T>.Default;
        }

        public void Signal()
        {
            OnChange?.Invoke(currentState);
        }

        public T CurrentValue
        {
            get => currentState;
            set
            {
                if (value == null && currentState == null)
                    return;

                if (value == null || !comparer.Equals(currentState, value))
                {
                    currentState = value;
                    OnChange?.Invoke(currentState);
                }
            }
        }

        public void Dispose()
            => OnChange = null;
    }

    public class ReactiveValueClamped<T> : IDisposable where T : IComparable<T>, IEquatable<T>
    {
        private T currentState;
        public T Min { get; }
        public T Max { get; private set; }

        public ReactiveValueClamped(T currentState, T min, T max)
        {
            this.currentState = currentState;
            Min = min;
            Max = max;
        }

        public event Action<T> OnChange;

        public void Signal()
        {
            OnChange?.Invoke(currentState);
        }

        public T CurrentValue
        {
            get => currentState;
            set
            {
                if (value.CompareTo(Max) > 0) value = Max;
                else if (value.CompareTo(Min) < 0) value = Min;

                if (value.Equals(currentState))
                    return;

                currentState = value;
                OnChange?.Invoke(currentState);
            }
        }

        public void SetMax(T value)
            => Max = value;

        public void Dispose()
            => OnChange = null;
    }
}