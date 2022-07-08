using System;

namespace HECSFramework.Core
{
    public interface IMofifiersProcessor<T> : IResetModifiers where T : struct
    {
        void AddModifier(Guid owner, IModifier<T> modifier);
        void AddUniqueModifier(Guid owner, IModifier<T> modifier);
        void RemoveModifier(Guid owner, IModifier<T> modifier);
    }

    public interface IMofifiersCompositeProcessor<T> : IResetModifiers where T : struct
    {
        void AddModifier(int key, Guid owner, IModifier<T> modifier);
        void AddUniqueModifier(int key, Guid owner, IModifier<T> modifier);
        void RemoveModifier(int key, Guid owner, IModifier<T> modifier);
    }

    public interface IResetModifiers
    {
        void Reset();
    }

    public interface IOutCompositeModifier<T> : IMofifiersCompositeProcessor<T> where T : struct
    {
        void Calculate(int key, ref T value);
    }
}