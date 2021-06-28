using HECSFramework.Core.Helpers;
using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class ModifiersContainer<T, U> where U : struct where T : IModifier<U>
    {
        private readonly Dictionary<int, HashSet<T>> modifiers = new Dictionary<int, HashSet<T>>()
        {
            {(int)ModifierCalculationType.Add, new HashSet<T>()},
            {(int)ModifierCalculationType.Subtract, new HashSet<T>()},
            {(int)ModifierCalculationType.Multiply, new HashSet<T>()},
            {(int)ModifierCalculationType.Divide, new HashSet<T>()},
        };

        public bool Contains(Func<T,bool> predicate)
        {
            foreach (var modifier in modifiers)
            {
                foreach (var value in modifier.Value)
                {
                    if (predicate(value))
                        return true;
                }
            }

            return false;
        }

        public void AddModifier(T modifier)
        {
            modifiers[(int)modifier.GetCalculationType].AddOrRemoveElement(modifier, true);
        }

        public void RemoveModifier(T modifier)
        {
            modifiers[(int)modifier.GetCalculationType].AddOrRemoveElement(modifier, false);
        }

        public U GetCalculatedValue(in U value)
        {
            U currentMod = value;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Add])
                valueMod.Modify(ref currentMod);

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Subtract])
                valueMod.Modify(ref currentMod);

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Multiply])
                valueMod.Modify(ref currentMod);

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Divide])
                valueMod.Modify(ref currentMod);

            return currentMod;
        }
    }
}
