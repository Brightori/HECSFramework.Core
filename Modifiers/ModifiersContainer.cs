using HECSFramework.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core
{
    //todo вернуть кэшированное значение и разметку грязный
    public class ModifiersContainer<T, U> where U : struct where T : IModifier<U>
    {
        private readonly Dictionary<int, HashSet<T>> modifiers = new Dictionary<int, HashSet<T>>()
        {
            {(int)ModifierCalculationType.Add, new HashSet<T>()},
            {(int)ModifierCalculationType.Subtract, new HashSet<T>()},
            {(int)ModifierCalculationType.Multiply, new HashSet<T>()},
            {(int)ModifierCalculationType.Divide, new HashSet<T>()},
        };

        public IReadOnlyDictionary<int, HashSet<T>> Modifiers => modifiers;

        public bool Contains(Func<T, bool> predicate)
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

        public void AddUniqueModifier(T modifier)
        {
            if (modifiers[(int)modifier.GetCalculationType].Any(x => x.ModifiersOwner == modifier.ModifiersOwner))
                return;

            AddModifier(modifier);
        }

        public void AddModifier(T modifier)
        {
            modifiers[(int)modifier.GetCalculationType].AddOrRemoveElement(modifier, true);
        }

        public void RemoveModifier(T modifier)
        {
            modifiers[(int)modifier.GetCalculationType].AddOrRemoveElement(modifier, false);
        }  
        
        public void RemoveModifier(Guid modifierOwner)
        {
            foreach (var m in modifiers)
            {
                foreach (var mf in m.Value.ToArray())
                    if (mf.ModifiersOwner == modifierOwner)
                    {
                        m.Value.Remove(mf);
                    }
            }
        }

        public U GetCalculatedValue(U value)
        {
            U currentMod = value;

            //todo оч странное решение, надо пересмотреть, мы не гарантируем что внутри контейнера заявленная арифметическая операция
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

        public void Clear()
        {
            foreach (var kvp in modifiers) kvp.Value.Clear();
        }
    }
}
