using System.Collections.Generic;

//same registration scheme as TypeContainersRegistry, see the comments there
public static partial class EntityContainersMap
{
    public static readonly Dictionary<int, string> EntityContainersIDtoString;

    private static Dictionary<int, string> collected;

    static EntityContainersMap()
    {
        EntityContainersIDtoString = collected ?? new Dictionary<int, string>();
        collected = null;
    }

    private static bool Register(int containerIndex, string name)
    {
        (collected ??= new Dictionary<int, string>())[containerIndex] = name;
        return true;
    }
}

public static partial class StrategiesMap
{
    public static readonly Dictionary<int, string> StrategiesIDtoString;

    private static Dictionary<int, string> collected;

    static StrategiesMap()
    {
        StrategiesIDtoString = collected ?? new Dictionary<int, string>();
        collected = null;
    }

    private static bool Register(int strategyIndex, string name)
    {
        (collected ??= new Dictionary<int, string>())[strategyIndex] = name;
        return true;
    }
}
