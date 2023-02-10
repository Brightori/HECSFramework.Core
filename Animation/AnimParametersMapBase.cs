using System.Collections.Generic;
using HECSFramework.Core;

[Documentation(Doc.Animation, Doc.HECS, Doc.GameLogic, "here we store string keys with unity animator hash, we need it to set parameters to animator")]
public static partial class AnimParametersMap
{
    public static readonly Dictionary<string, int> AnimParameters;
}