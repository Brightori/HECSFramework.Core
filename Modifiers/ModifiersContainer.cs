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

        protected readonly Dictionary<int, List<OwnerModifier>> modifiers = new Dictionary<int, List<OwnerModifier>>(4)
        {
            {(int)ModifierCalculationType.Add, new List<OwnerModifier>(4)},
            {(int)ModifierCalculationType.Subtract, new List<OwnerModifier>(4)},
            {(int)ModifierCalculationType.Multiply, new List<OwnerModifier>(4)},
            {(int)ModifierCalculationType.Divide, new List<OwnerModifier>(4)},
        };

        private Queue<OwnerModifier> cleanQueue = new Queue<OwnerModifier>(2);
        private Queue<CleanModifier> removedModifiers = new Queue<CleanModifier>(4);

        public Data GetForceCalculatedValue()
        {
            isDirty = true;
            GetCalculatedValue();
            return calculatedValue;
        }

        public void SetBaseValue(Data data)
        {
            baseValue = data;
            isDirty = true;
        }

        public IReadOnlyDictionary<int, List<OwnerModifier>> Modifiers => modifiers;

        public IEnumerable<IModifier<Data>> GetModifiers()
        {
            foreach (var modifier in modifiers)
            {
                foreach (var m in modifier.Value)
                    yield return m.Modifier;
            }
        }

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

        public void RemoveModifier(Guid owner, IModifier<Data> modifier, bool unique = false)
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

                    if (unique)
                        break;
                }
            }

            CleanUpRemovedModifiers();
            isDirty = true;
        }

        public void SetDirty()
        {
            isDirty = true;
        }
        

        public void RemoveModifier(Guid modifierGUID, bool unique = false)
        {
            foreach (var collection in modifiers)
            {
                foreach (var currentmodifier in collection.Value)
                {
                    if (currentmodifier.Modifier.ModifierGuid == modifierGUID)
                    {
                        removedModifiers.Enqueue(new CleanModifier
                        {
                            OwnerModifier = currentmodifier,
                            TypeOfModifier = (int)currentmodifier.Modifier.GetModifierType,
                        });

                        if (unique)
                            break;
                    }
                }
            }

            CleanUpRemovedModifiers();
            isDirty = true;
        }

        public void RemoveModifier(int modifierID, bool unique = false)
        {
            foreach (var collection in modifiers)
            {
                foreach (var currentmodifier in collection.Value)
                {
                    if (currentmodifier.Modifier.ModifierID == modifierID)
                    {
                        removedModifiers.Enqueue(new CleanModifier
                        {
                            OwnerModifier = currentmodifier,
                            TypeOfModifier = (int)currentmodifier.Modifier.GetModifierType,
                        });

                        if (unique)
                            break;
                    }
                }
            }

            CleanUpRemovedModifiers();
            isDirty = true;
        }

        public void Reset()
        {
            Clear();
            isDirty = true;
            calculatedValue = baseValue;
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
            removedModifiers.Clear();
            cleanQueue.Clear();
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