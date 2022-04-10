using System;

namespace HECSFramework.Core
{
    public interface IModifier
    {
        Guid ModifierGuid { get; }
        ModifierCalculationType GetCalculationType { get; }
        ModifierValueType GetModifierType { get; }
    }

    public interface IModifier<T> : IModifier where T : struct
    {
        T GetValue { get; }
        void Modify(ref T currentMod);
    }

    public enum ModifierCalculationType
    {
        Add = 0,
        Subtract = 1,
        Multiply = 2,
        Divide = 3,
    }

    public enum ModifierValueType
    {
        Value = 0,
        Percent = 1
    }
}