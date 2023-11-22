using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    public partial class ModifiableFloatCounter :  ICounterModifiable<float>, IDisposable
    {
        private float currentValue;
        public float Value { get => currentValue; private set => currentValue = value; }
        public float CalculatedMaxValue => modifiersContainer.GetCalculatedValue();
        public int Id { get; private set; }

        protected ModifiersFloatContainer modifiersContainer = new ModifiersFloatContainer();

        public void Setup (int key, float baseValue)
        {
            Id = key;
            modifiersContainer.SetBaseValue(baseValue);
            currentValue = baseValue;
        }

        public void AddModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = currentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);
            modifiersContainer.GetCalculatedValue();

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void RemoveModifier(Guid owner, IModifier<float> modifier, bool unique = false)
        {
            var oldValue = currentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(owner, modifier, unique);

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void RemoveModifier(Guid modifier, bool unique = false)
        {
            var oldValue = currentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(modifier, unique);
            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void RemoveModifier(int modifierID, bool unique = false)
        {
            var oldValue = currentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(modifierID, unique);
            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void AddUniqueModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = currentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void SetValue(float value)
        {
            currentValue = value;
        }

        public void ChangeValue(float value)
        {
            var upd = currentValue + value;
            var calc = modifiersContainer.GetCalculatedValue();

            if (upd > calc)
                currentValue = modifiersContainer.GetCalculatedValue();
            else
                currentValue = upd;
        }

        private void UpdatValueWithModifiers(float oldValue, float oldCalculated)
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
            currentValue = modifiersContainer.GetCalculatedValue();
        }

        public IEnumerable<IModifier<float>> GetModifiers() => modifiersContainer.GetModifiers();

        public void SetIsDirty()
        {
            modifiersContainer.SetDirty();
        }
    }
}