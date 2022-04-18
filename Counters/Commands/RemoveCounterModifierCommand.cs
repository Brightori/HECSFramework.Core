using System;
using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.Counters, Doc.Modifiers, "Command for remove modifier")]
    public struct RemoveCounterModifierCommand<T> : ICommand where T : struct
    {
        public int Id;
        public Guid Owner;
        public IModifier<T> Modifier;
    }

    [Documentation(Doc.HECS, Doc.Counters, Doc.Modifiers, "Command for remove complext modifier")]
    public struct RemoveComplexCounterModifierCommand<T> : ICommand where T : struct
    {
        public int Id;
        public int SubId;
        public Guid Owner;
        public IModifier<T> Modifier;
    }
}