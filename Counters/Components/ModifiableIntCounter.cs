using System;
using HECSFramework.Core;

namespace Components
{
    public partial class ModifiableIntCounter : ICounterModifiable<int>, IDisposable
    {
        public int Value => modifiersContainer.CurrentValue;
        public int CalculatedMaxValue => modifiersContainer.GetCalculatedValue();
        public int Id { get; }

        protected ModifiersContainer<IModifier<int>, int> modifiersContainer;

        public ModifiableIntCounter(int key, int baseValue)
        {
            Id = key;
            modifiersContainer = new ModifiersContainer<IModifier<int>, int>(baseValue);
        }

        public void AddModifier(Guid owner, IModifier<int> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);
            modifiersContainer.GetCalculatedValue();

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void RemoveModifier(Guid owner, IModifier<int> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(owner, modifier);

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void AddUniqueModifier(Guid owner, IModifier<int> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);

            UpdatValueWithModifiers(oldValue, oldCalculated);
        }

        public void SetValue(int value)
        {
            var oldValue = Value;
            modifiersContainer.SetCurrentValue(value);
        }

        public void ChangeValue(int value)
        {
            var upd = modifiersContainer.CurrentValue + value;

            if (upd > modifiersContainer.GetCalculatedValue())
                modifiersContainer.SetCurrentValue(modifiersContainer.GetCalculatedValue());
            else
                modifiersContainer.SetCurrentValue(upd);
        }

        private void UpdatValueWithModifiers(int oldValue, int oldCalculated)
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