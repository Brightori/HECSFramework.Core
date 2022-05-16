using System;

namespace HECSFramework.Core
{
    [Serializable]
    public sealed class DefaultFloatModifier : BaseModifier<float>
    {
        public override float GetValue { get; set; }
        public override ModifierCalculationType GetCalculationType { get; set; }
        public override ModifierValueType GetModifierType { get; set; }
        public override Guid ModifierGuid { get; set; }

        public override void Modify(ref float currentMod)
        {
            currentMod = ModifiersCalculation.GetResult(currentMod, GetValue, GetCalculationType, GetModifierType);
        }
    }

    [Serializable]
    public sealed class DefaultIntModifier : BaseModifier<int>
    {
        public override int GetValue { get; set; }
        public override ModifierCalculationType GetCalculationType { get; set; }
        public override ModifierValueType GetModifierType { get; set; }
        public override Guid ModifierGuid { get; set; }

        public override void Modify(ref int currentMod)
        {
            currentMod = ModifiersCalculation.GetResult(currentMod, GetValue, GetCalculationType, GetModifierType);
        }
    }

    public abstract class BaseModifier<T> : IModifier<T> where T: struct
    {
        public abstract T GetValue { get; set; }
        public abstract ModifierCalculationType GetCalculationType { get; set; }
        public abstract ModifierValueType GetModifierType { get; set; }
        public abstract Guid ModifierGuid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is BaseModifier<T> modifier &&
                   ModifierGuid.Equals(modifier.ModifierGuid);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ModifierGuid);
        }

        public abstract void Modify(ref T currentMod);
    }
}
