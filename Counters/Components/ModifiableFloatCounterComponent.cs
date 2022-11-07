using System;
using Commands;
using HECSFramework.Core;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Counters, "this component is base for all counters components with modifiable values. this component holds modifier container")]
    public abstract partial class ModifiableFloatCounterComponent : BaseComponent, ICounterModifiable<float>, IInitable, IDisposable
    {
        public virtual float Value => modifiersContainer.CurrentValue;
        public float CalculatedMaxValue => modifiersContainer.GetCalculatedValue();
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
            modifiersContainer.GetCalculatedValue();

            UpdatValueWithModifiers(oldValue, oldCalculated);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void RemoveModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(owner, modifier);

            UpdatValueWithModifiers(oldValue, oldCalculated);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void AddUniqueModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);

            UpdatValueWithModifiers(oldValue, oldCalculated);

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

            if (upd > modifiersContainer.GetCalculatedValue())
                modifiersContainer.SetCurrentValue(modifiersContainer.GetCalculatedValue());
            else
                modifiersContainer.SetCurrentValue(upd);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        private bool CheckModifiedDiff(float oldValue, out DiffCounterCommand<float> result)
        {
            if (oldValue != Value)
            {
                result = new DiffCounterCommand<float> { Id = this.Id, Value = modifiersContainer.CurrentValue, PreviousValue = oldValue, MaxValue = modifiersContainer.GetCalculatedValue() };
                return true;
            }

            result = default;
            return false;
        }

        private void UpdatValueWithModifiers(float oldValue, float oldCalculated)
        {
            var percent = oldCalculated > 0 ? oldValue / oldCalculated : 1;
            modifiersContainer.SetCurrentValue(modifiersContainer.GetCalculatedValue()*percent);
        }

        public void Dispose()
        {
            modifiersContainer?.Clear();
        }

        public void Reset()
        {
            modifiersContainer.Reset();
        }
    }
}