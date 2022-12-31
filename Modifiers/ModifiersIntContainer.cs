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
            var currentMod = value;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Add])
            {
                var calc = baseForCalculation;
                valueMod.Modifier.Modify(ref calc);
                currentMod += calc;
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