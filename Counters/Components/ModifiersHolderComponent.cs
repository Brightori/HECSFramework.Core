using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Counters, Doc.Modifiers, "here we hold modifiers for saving or iteration purpose, from counters holder system")]
    public sealed class ModifiersHolderComponent : BaseComponent
    {
        public List<IModifier<int>> IntModifiers = new List<IModifier<int>>(0);
        public List<IModifier<float>> FloatModifiers = new List<IModifier<float>>(0);
    }
}
