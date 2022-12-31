using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core
{

    public abstract partial class ModifiersContainer<Data> where Data : struct 
    {
        public struct CleanModifier
        {
            public int TypeOfModifier;
            public OwnerModifier OwnerModifier;
        }

        public struct OwnerModifier : IEquatable<OwnerModifier>
        {
            public Guid ModifiersOwner;
            public IModifier<Data> Modifier;

            public override bool Equals(object obj)
            {
                return obj is OwnerModifier modifier &&
                       modifier.ModifiersOwner == ModifiersOwner && Equals(modifier);
            }

            public bool Equals(OwnerModifier other)
            {
                return other.ModifiersOwner == ModifiersOwner && Modifier.ModifierGuid == other.Modifier.ModifierGuid;
            }

            public override int GetHashCode()
            {
                int hashCode = 2083788928;
                hashCode = hashCode * -1521134295 + ModifiersOwner.GetHashCode();
                hashCode = hashCode * -1521134295 + Modifier.ModifierGuid.GetHashCode();
                return hashCode;
            }
        }

        protected Data baseValue;
        protected Data calculatedValue;
        protected bool isDirty;

        protected readonly Dictionary<int, List<OwnerModifier>> modifiers = new Dictionary<int, List<OwnerModifier>>()
        {
            {(int)ModifierCalculationType.Add, new List<OwnerModifier>()},
            {(int)ModifierCalculationType.Subtract, new List<OwnerModifier>()},
            {(int)ModifierCalculationType.Multiply, new List<OwnerModifier>()},
            {(int)ModifierCalculationType.Divide, new List<OwnerModifier>()},
        };

        private Queue<OwnerModifier> cleanQueue = new Queue<OwnerModifier>();
        private Queue<CleanModifier> removedModifiers = new Queue<CleanModifier>(4);

        public void SetBaseValue(Data data)
        {
            baseValue = data;
            isDirty = true;
        }

        public IReadOnlyDictionary<int, List<OwnerModifier>> Modifiers => modifiers;

        public bool Contains(Func<IModifier<Data>, bool> predicate)
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

        public void AddUniqueModifier(Guid owner, IModifier<Data> modifier)
        {
            if (modifiers[(int)modifier.GetCalculationType].Any(x => x.ModifiersOwner == owner || x.Modifier.ModifierGuid == modifier.ModifierGuid))
                return;

            AddModifier(owner, modifier);
            isDirty = true;
        }

        public void AddModifier(Guid owner, IModifier<Data> modifier)
        {
            modifiers[(int)modifier.GetCalculationType].Add(new OwnerModifier { Modifier = modifier, ModifiersOwner = owner });
            isDirty = true;
        }

        public void RemoveModifier(Guid owner, IModifier<Data> modifier)
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

        public abstract Data GetCalculatedValue();
        public abstract Data GetCalculatedValue(Data value);
       

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