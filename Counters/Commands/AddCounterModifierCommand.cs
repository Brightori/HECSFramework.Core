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
}