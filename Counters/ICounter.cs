using System;

namespace HECSFramework.Core
{
    public interface ICounter
    {
        int Id { get; }
    }

    public interface ICounter<T> : ICounter
    {
        T Value { get; }
        void SetValue(T value);
        void ChangeValue(T value);
    }

    public interface IBaseValue<T>
    {
        T GetBaseValue { get; }
    }

    public interface IMaxValue<T>
    {
        T MaxValue { get; }
    }

    public interface ICounterModifiable<T> : ICounter<T> where T : struct
    {
        void AddModifier(Guid owner, IModifier<T> modifier);
        void AddUniqueModifier(Guid owner, IModifier<T> modifier);
        void RemoveModifier(Guid owner, IModifier<T> modifier);
    }
}
