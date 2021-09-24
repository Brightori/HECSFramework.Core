namespace HECSFramework.Core
{
    public static class ModifiersCalculation
    {
        public static float GetResult(float modifiedValue, in float parametr, ModifierCalculationType modifierCalculationType, ModifierValueType modifierValueType)
        {
            switch (modifierValueType)
            {
                case ModifierValueType.Value:
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
                    break;
                case ModifierValueType.Percent:
                    var percent = modifiedValue / 100;
                    switch (modifierCalculationType)
                    {
                        case ModifierCalculationType.Add:
                            return modifiedValue + percent * parametr;
                        case ModifierCalculationType.Subtract:
                            return modifiedValue - percent * parametr;
                        case ModifierCalculationType.Multiply:
                            return modifiedValue * percent * parametr;
                        case ModifierCalculationType.Divide:
                            if (parametr > 0)
                                return modifiedValue / (percent * parametr);
                            break;
                    }
                    break;
            }


            return -1;
        }

        public static int GetResult(in int modifiedValue, in int parametr, ModifierCalculationType modifierCalculationType, ModifierValueType modifierValueType)
        {
            switch (modifierValueType)
            {
                case ModifierValueType.Value:
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
                    break;
                case ModifierValueType.Percent:
                    var percent = modifiedValue / 100;
                    switch (modifierCalculationType)
                    {
                        case ModifierCalculationType.Add:
                            return modifiedValue + percent * parametr;
                        case ModifierCalculationType.Subtract:
                            return modifiedValue - percent * parametr;
                        case ModifierCalculationType.Multiply:
                            return modifiedValue * percent * parametr;
                        case ModifierCalculationType.Divide:
                            if (parametr > 0)
                                return modifiedValue / (percent * parametr);
                            break;
                    }
                    break;
            }
            return -1;
        }
    }
}
