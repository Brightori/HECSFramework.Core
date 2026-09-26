using System.Collections.Generic;

//same registration scheme as TypeContainersRegistry, see the comments there
public static partial class AbilitiesMap
{
    public static readonly Dictionary<string, int> AbilitiesToIdentifiersMap;

    private static Dictionary<string, int> collected;

    static AbilitiesMap()
    {
        AbilitiesToIdentifiersMap = collected ?? new Dictionary<string, int>();
        collected = null;
    }

    private static bool Register(string name, int containerIndex)
    {
        (collected ??= new Dictionary<string, int>())[name] = containerIndex;
        return true;
    }
}
