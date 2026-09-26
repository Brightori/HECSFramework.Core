using System.Collections.Generic;

//same registration scheme as TypeContainersRegistry, see the comments there
public static partial class IdentifierToStringMap
{
    public static readonly Dictionary<int, string> IntToString;

    private static Dictionary<int, string> collected;

    static IdentifierToStringMap()
    {
        IntToString = collected ?? new Dictionary<int, string>();
        collected = null;
    }

    private static bool Register(int id, string name)
    {
        (collected ??= new Dictionary<int, string>())[id] = name;
        return true;
    }
}
