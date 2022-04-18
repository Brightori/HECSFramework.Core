using System;
using Commands;
using HECSFramework.Core;

namespace Components
{
    public partial class ModifiableFloatCounter :  ICounterModifiable<float>, IDisposable
    {
        public float Value => modifiersContainer.CurrentValue;
        public float CalculatedMaxValue => modifiersContainer.GetCalculatedValue();
        public int Id { get; }

        protected ModifiersContainer<IModifier<float>, float> modifiersContainer;

        public ModifiableFloatCounter (int key, float baseValue)
        {
            Id = key;
            modifiersContainer = new ModifiersContainer<IModifier<float>, float>(baseValue);
        }

        public void AddModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);
            modifiersContainer.GetCalculatedValue();

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void RemoveModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(owner, modifier);

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void AddUniqueModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void SetValue(float value)
        {
            var oldValue = Value;
            modifiersContainer.SetCurrentValue(value);
        }

        public void ChangeValue(float value)
        {
            var upd = modifiersContainer.CurrentValue + value;

            if (upd > modifiersContainer.GetCalculatedValue())
                modifiersContainer.SetCurrentValue(modifiersContainer.GetCalculatedValue());
            else
                modifiersContainer.SetCurrentValue(upd);
        }

        private void UpdatValueWithModifiers(float oldValue, float oldCalculated)
        {
            var percent = oldCalculated > 0 ? oldValue / oldCalculated : 1;
            modifiersContainer.SetCurrentValue(modifiersContainer.GetCalculatedValue() * percent);
        }

        public void Dispose()
        {
            modifiersContainer.Clear();
        }

        public void Reset()
        {
            modifiersContainer.Reset();
        }
    }
}