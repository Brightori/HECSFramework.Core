using System;
using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Modifiers, "Add complex modifier local command")]
	public struct AddComplexCounterModifierCommand<T> : ICommand where T : struct
	{
		public int Id;
		public int SubId;
		public Guid Owner;
		public IModifier<T> Modifier;
		public bool IsUnique;
	}
}