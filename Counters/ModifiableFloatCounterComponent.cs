using System;
using Commands;
using HECSFramework.Core;

namespace Components
{
    public abstract partial class ModifiableFloatCounterComponent : BaseComponent, ICounterModifiable<float>, IInitable, IDisposable
    {
        public float Value => modifiersContainer.CurrentValue;
        public abstract int Id { get; }
        public abstract float SetupValue { get; }

        protected ModifiersContainer<IModifier<float>, float> modifiersContainer;

        public void Init()
        {
            modifiersContainer = new ModifiersContainer<IModifier<float>, float>(SetupValue);
        }

        public void AddModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();
            
            modifiersContainer.AddModifier(owner, modifier);
            UpdatValueWithModifiers(Value, oldCalculated);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void RemoveModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(owner, modifier);

            UpdatValueWithModifiers(Value, oldCalculated);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void AddUniqueModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);

            UpdatValueWithModifiers(Value, oldCalculated);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void SetValue(float value)
        {
            var oldValue = Value;
            modifiersContainer.SetCurrentValue(value);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void ChangeValue(float value)
        {
            var oldValue = Value;
            var upd = modifiersContainer.CurrentValue + value;
            modifiersContainer.SetCurrentValue(upd);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        private bool CheckModifiedDiff(float oldValue, out DiffCounterCommand<float> result)
        {
            if (oldValue != Value)
            {
                result = new DiffCounterCommand<float> { Id = this.Id, Diff = Value - oldValue, PreviousValue = oldValue };
                return true;
            }

            result = default;
            return false;
        }

        private void UpdatValueWithModifiers(float oldValue, float oldCalculated)
        {
            var percent = oldValue / oldCalculated;
            modifiersContainer.SetCurrentValue(modifiersContainer.GetCalculatedValue()*percent);
        }

        public void Dispose()
        {
            modifiersContainer.Clear();
        }
    }
}