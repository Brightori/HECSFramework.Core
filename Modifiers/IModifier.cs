using System;

namespace HECSFramework.Core
{
    public interface IModifier
    {
        /// <summary>
        /// we should save here counter id. for saving purpose and associate modifier to counter
        /// </summary>
        int ModifierCounterID { get; set; }
        
        /// <summary>
        /// this parameter should be use for saving puropose, we provide here ModifierTypeIdentifier
        /// </summary>
        int ModifierType { get; set; }

        /// <summary>
        /// we can separate instances of modifiers by guid
        /// </summary>
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