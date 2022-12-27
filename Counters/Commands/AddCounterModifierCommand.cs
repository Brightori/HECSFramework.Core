using System;
using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Modifiers, "Add Modifier local command")]
	public struct AddCounterModifierCommand<T> : ICommand where T: struct
	{
		public int Id;
		public Guid Owner;
		public IModifier<T> Modifier;
		public bool IsUnique;
	}

    [Documentation(Doc.Modifiers, "This command looking for counter by sub id and add modifier to this counter")]
    public struct AddCounterModifierBySubIDCommand<T> : ICommand where T : struct
    {
        public int Id;
        public Guid Owner;
        public IModifier<T> Modifier;
        public bool IsUnique;
    }
}