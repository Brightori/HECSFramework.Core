using System;

namespace HECSFramework.Core
{
    public sealed partial class BaseValueModifiersFloatContainer : ModifiersContainer<float>
    {
        public override float GetCalculatedValue()
        {
            if (!isDirty)
                return calculatedValue;

            isDirty = false;
            calculatedValue = GetCalculatedValue(baseValue);
            return calculatedValue;
        }

        public override float GetCalculatedValue(float value)
        {
            var baseForCalculation = value;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Add])
            {
                if (valueMod.Modifier.GetModifierType == ModifierValueType.Percent)
                {
                    var processValue = value;
                    valueMod.Modifier.Modify(ref processValue);
                    baseForCalculation += (processValue - value);
                }
                else
                {
                    valueMod.Modifier.Modify(ref baseForCalculation);
                }
            }

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Subtract])
            {
                if (valueMod.Modifier.GetModifierType == ModifierValueType.Percent)
                {
                    var processValue = value;
                    valueMod.Modifier.Modify(ref processValue);
                    baseForCalculation -= processValue;
                }
                else
                {
                    valueMod.Modifier.Modify(ref baseForCalculation);
                }
            }

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Multiply])
            {
                if (valueMod.Modifier.GetValue > 1)
                {
                    var processValue = value;
                    valueMod.Modifier.Modify(ref processValue);
                    baseForCalculation += processValue;
                }
                else
                {
                    var processValue = value;
                    valueMod.Modifier.Modify(ref processValue);
                    baseForCalculation += processValue;
                }
            }

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Divide])
            {
                var processValue = value;
                valueMod.Modifier.Modify(ref processValue);
                baseForCalculation -= processValue;
            }

            return baseForCalculation;
        }
    }
}