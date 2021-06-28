namespace HECSFramework.Core
{
    public struct ModifiersCalculation
    {
        public float GetResult(float modifiedValue, in float parametr, ModifierCalculationType modifierCalculationType)
        {
            switch (modifierCalculationType)
            {
                case ModifierCalculationType.Add:
                    return modifiedValue + parametr;
                case ModifierCalculationType.Subtract:
                    return modifiedValue - parametr;
                case ModifierCalculationType.Multiply:
                    return modifiedValue * parametr;
                case ModifierCalculationType.Divide:
                    if (parametr > 0)
                        return modifiedValue / parametr;
                    break;
            }

            return -1;
        } 
        
        public int GetResult(in int modifiedValue, in int parametr, ModifierCalculationType modifierCalculationType)
        {
            switch (modifierCalculationType)
            {
                case ModifierCalculationType.Add:
                    return modifiedValue + parametr;
                case ModifierCalculationType.Subtract:
                    return modifiedValue - parametr;
                case ModifierCalculationType.Multiply:
                    return modifiedValue * parametr;
                case ModifierCalculationType.Divide:
                    if (parametr > 0)
                        return modifiedValue / parametr;
                    break;
            }

            return -1;
        }
    }
}
