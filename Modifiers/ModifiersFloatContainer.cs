using System;

namespace HECSFramework.Core
{
    public sealed partial class ModifiersFloatContainer : ModifiersContainer<float>
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
            var currentMod = 0f;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Add])
            {
                var calc = baseForCalculation;
                valueMod.Modifier.Modify(ref calc);
                currentMod += calc;
            }

            baseForCalculation = Math.Abs(currentMod - baseForCalculation);
            currentMod = 0;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Subtract])
            {
                valueMod.Modifier.Modify(ref baseForCalculation);
            }

            baseForCalculation += currentMod;
            currentMod = baseForCalculation;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Multiply])
            {
                valueMod.Modifier.Modify(ref currentMod);
            }
                
            baseForCalculation = currentMod;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Divide])
            {
                valueMod.Modifier.Modify(ref baseForCalculation);
            }

            return baseForCalculation;
        }
    }
}