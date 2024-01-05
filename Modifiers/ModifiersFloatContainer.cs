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