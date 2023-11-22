using System;
using System.Collections.Generic;
using HECSFramework.Core;
using static UnityEngine.UI.GridLayoutGroup;

namespace Components
{
    public partial class ModifiableIntCounter : ICounterModifiable<int>, IDisposable
    {
        private int currentValue;
        public int Value => currentValue;
        public int CalculatedMaxValue => modifiersContainer.GetCalculatedValue();
        public int Id { get; private set; }

        protected ModifiersIntContainer modifiersContainer = new ModifiersIntContainer();

        public void Setup(int key, int baseValue)
        {
            Id = key;
            modifiersContainer.SetBaseValue(baseValue);
            currentValue = baseValue;
        }

        public void AddModifier(Guid owner, IModifier<int> modifier)
        {
            var oldValue = currentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);
            modifiersContainer.GetCalculatedValue();

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void RemoveModifier(Guid owner, IModifier<int> modifier, bool unique = false)
        {
            var oldValue = currentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(owner, modifier, unique);

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void RemoveModifier(Guid modifierGUID, bool unique = false)
        {
            var oldValue = currentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(modifierGUID, unique);

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void RemoveModifier(int modifierID, bool unique = false)
        {
            var oldValue = currentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(modifierID, unique);

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void AddUniqueModifier(Guid owner, IModifier<int> modifier)
        {
            var oldValue = currentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void SetValue(int value)
        {
           currentValue = value;
        }

        public void ChangeValue(int value)
        {
            var upd = currentValue + value;
            var calc = modifiersContainer.GetCalculatedValue();

            if (upd > calc)
                currentValue = calc;
            else
                currentValue = upd;
        }

        private void UpdatValueWithModifiers(int oldValue, int oldCalculated)
        {
            var percent = oldCalculated > 0 ? oldValue / oldCalculated : 1;
            currentValue = (modifiersContainer.GetCalculatedValue() * percent);
        }

        public void Dispose()
        {
            modifiersContainer.Clear();
        }

        public void Reset()
        {
            modifiersContainer.Reset();
        }

        public IEnumerable<IModifier<int>> GetModifiers() => modifiersContainer.GetModifiers();

        public void SetIsDirty()
        {
            modifiersContainer.SetDirty();
        }
    }
}