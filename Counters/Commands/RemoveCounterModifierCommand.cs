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
}  