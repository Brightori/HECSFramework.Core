using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core
{
    public sealed partial class ModifiersContainer<T, U> where U : struct where T : IModifier<U>
    {
        public struct CleanModifier
        {
            public int TypeOfModifier;
            public OwnerModifier OwnerModifier;
        }

        public struct OwnerModifier
        {
            public Guid ModifiersOwner;
            public T Modifier;

            public override bool Equals(object obj)
            {
                return obj is OwnerModifier modifier &&
                       ModifiersOwner.Equals(modifier.ModifiersOwner) &&
                       EqualityComparer<T>.Default.Equals(Modifier, modifier.Modifier);
            }

            public override int GetHashCode()
            {
                int hashCode = 2083788928;
                hashCode = hashCode * -1521134295 + ModifiersOwner.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(Modifier);
                return hashCode;
            }
        }

        public U CurrentValue { get; private set; }
        private U baseValue;
        private U calculatedValue;
        private bool isDirty;

        private readonly Dictionary<int, List<OwnerModifier>> modifiers = new Dictionary<int, List<OwnerModifier>>()
        {
            {(int)ModifierCalculationType.Add, new List<OwnerModifier>()},
            {(int)ModifierCalculationType.Subtract, new List<OwnerModifier>()},
            {(int)ModifierCalculationType.Multiply, new List<OwnerModifier>()},
            {(int)ModifierCalculationType.Divide, new List<OwnerModifier>()},
        };

        private Queue<OwnerModifier> cleanQueue = new Queue<OwnerModifier>();
        private Queue<CleanModifier> removedModifiers = new Queue<CleanModifier>(4);

        public ModifiersContainer(U baseValue)
        {
            this.baseValue = baseValue;
            CurrentValue = baseValue;
            calculatedValue = baseValue;
        }

        public IReadOnlyDictionary<int, List<OwnerModifier>> Modifiers => modifiers;

        public bool Contains(Func<T, bool> predicate)
        {
            foreach (var modifier in modifiers)
            {
                foreach (var value in modifier.Value)
                {
                    if (predicate(value.Modifier))
                        return true;
                }
            }

            return false;
        }

        public void SetCurrentValue(U value)
        {
            CurrentValue = value;
        }

        public void AddUniqueModifier(Guid owner, T modifier)
        {
            if (modifiers[(int)modifier.GetCalculationType].Any(x => x.ModifiersOwner == owner || x.Modifier.ModifierGuid == modifier.ModifierGuid))
                return;

            AddModifier(owner, modifier);
            isDirty = true;
        }

        public void AddModifier(Guid owner, T modifier)
        {
            modifiers[(int)modifier.GetCalculationType].Add(new OwnerModifier { Modifier = modifier, ModifiersOwner = owner });
            isDirty = true;
        }

        public void RemoveModifier(Guid owner, T modifier)
        {
            foreach (var currentmodifier in modifiers[(int)modifier.GetCalculationType])
            {
                if (currentmodifier.Modifier.ModifierGuid == modifier.ModifierGuid && currentmodifier.ModifiersOwner == owner)
                {
                    removedModifiers.Enqueue(new CleanModifier
                    {
                        OwnerModifier = currentmodifier,
                        TypeOfModifier = (int)modifier.GetCalculationType,
                    });
                }
            }

            CleanUpRemovedModifiers();
            isDirty = true;
        }

        public void Reset()
        {
            Clear();
            CurrentValue = baseValue;
            isDirty = true;
        }

        public void RemoveModifier(Guid modifierOwner)
        {
            foreach (var m in modifiers)
            {
                foreach (var mf in m.Value)
                {
                    if (mf.ModifiersOwner == modifierOwner)
                        cleanQueue.Enqueue(mf);
                }

                while (cleanQueue.Count > 0)
                    m.Value.Remove(cleanQueue.Dequeue());
            }

            isDirty = true;
        }

        public U GetCalculatedValue()
        {
            if (!isDirty)
                return calculatedValue;

            var currentMod = baseValue;

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Add])
                valueMod.Modifier.Modify(ref currentMod);

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Subtract])
                valueMod.Modifier.Modify(ref currentMod);

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Multiply])
                valueMod.Modifier.Modify(ref currentMod);

            foreach (var valueMod in modifiers[(int)ModifierCalculationType.Divide])
                valueMod.Modifier.Modify(ref currentMod);

            isDirty = false;
            calculatedValue = currentMod;
            return calculatedValue;
        }

        public void Clear()
        {
            foreach (var kvp in modifiers) kvp.Value.Clear();
        }

        private void CleanUpRemovedModifiers()
        {
            while (removedModifiers.Count > 0)
            {
                var remModif = removedModifiers.Dequeue();
                modifiers[remModif.TypeOfModifier].Remove(remModif.OwnerModifier);
            }
        }
    }
}