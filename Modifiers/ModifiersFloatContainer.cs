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
            var currentMod = value;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Add])
            {
                var calc = baseForCalculation;
                valueMod.Modifier.Modify(ref calc);
                currentMod += (calc - baseForCalculation);
            }

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Subtract])
                valueMod.Modifier.Modify(ref currentMod);

            baseForCalculation = currentMod;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Multiply])
            {
                var calc = baseForCalculation;
                valueMod.Modifier.Modify(ref calc);
                currentMod += (calc - baseForCalculation);
            }
                
            baseForCalculation = currentMod;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Divide])
            {
                var calc = baseForCalculation;
                valueMod.Modifier.Modify(ref calc);
                currentMod = calc < currentMod ? calc : currentMod;
            }

            return currentMod;
        }
    }
}