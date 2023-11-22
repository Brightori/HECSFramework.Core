using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public interface IMofifiersProcessor<T> : IResetModifiers where T : struct
    {
        void AddModifier(Guid owner, IModifier<T> modifier);
        void AddUniqueModifier(Guid owner, IModifier<T> modifier);
        void RemoveModifier(Guid owner, IModifier<T> modifier, bool unique = false);
        void RemoveModifier(Guid modifierGUID, bool unique = false);
        void RemoveModifier(int modifierID, bool unique = false);
        IEnumerable<IModifier<T>> GetModifiers();
        public void SetIsDirty();
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