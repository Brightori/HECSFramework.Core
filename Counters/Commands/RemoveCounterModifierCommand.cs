using System;
using HECSFramework.Core;

namespace Commands
{
    public struct RemoveCounterModifierCommand<T> : ICommand where T : struct
    {
        public int Id;
        public Guid Owner;
        public IModifier<T> Modifier;
    }
}