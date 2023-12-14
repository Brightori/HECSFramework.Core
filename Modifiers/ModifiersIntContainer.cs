using System;

namespace HECSFramework.Core
{
    public sealed partial class ModifiersIntContainer : ModifiersContainer<int>
    {
        public override int GetCalculatedValue()
        {
            if (!isDirty)
                return calculatedValue;

            isDirty = false;
            calculatedValue = GetCalculatedValue(baseValue);
            return calculatedValue;
        }

        public override int GetCalculatedValue(int value)
        {
            var baseForCalculation = value;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Add])
            {
                valueMod.Modifier.Modify(ref baseForCalculation);
            }

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Subtract])
            {
                valueMod.Modifier.Modify(ref baseForCalculation);
            }

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Multiply])
            {
                valueMod.Modifier.Modify(ref baseForCalculation);
            }

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Divide])
            {
                valueMod.Modifier.Modify(ref baseForCalculation);
            }

            return baseForCalculation;
        }
    }
}