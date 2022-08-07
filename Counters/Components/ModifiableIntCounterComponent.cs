using System;
using Commands;
using HECSFramework.Core;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Counters, "this component is base for all counters components with modifiable values. this component holds modifier container")]
    public abstract partial class ModifiableIntCounterComponent : BaseComponent, IBaseValue<int>, ICounterModifiable<int>, IInitable, IDisposable
    {
        public int Value => modifiersContainer.CurrentValue;
        public int CalculatedMaxValue => modifiersContainer.GetCalculatedValue();
        public abstract int Id { get; }
        public abstract int SetupValue { get; }
        public int GetBaseValue => SetupValue;

        protected ModifiersContainer<IModifier<int>, int> modifiersContainer;

        public void Init()
        {
            modifiersContainer = new ModifiersContainer<IModifier<int>, int>(SetupValue);
        }

        public void AddModifier(Guid owner, IModifier<int> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);
            modifiersContainer.GetCalculatedValue();

            UpdatValueWithModifiers(oldValue, oldCalculated);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void RemoveModifier(Guid owner, IModifier<int> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.RemoveModifier(owner, modifier);

            UpdatValueWithModifiers(oldValue, oldCalculated);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void AddUniqueModifier(Guid owner, IModifier<int> modifier)
        {
            var oldValue = modifiersContainer.CurrentValue;
            var oldCalculated = modifiersContainer.GetCalculatedValue();

            modifiersContainer.AddModifier(owner, modifier);

            UpdatValueWithModifiers(oldValue, oldCalculated);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void SetValue(int value)
        {
            var oldValue = Value;
            modifiersContainer.SetCurrentValue(value);

            if (CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void ChangeValue(int value)
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

        private bool CheckModifiedDiff(int oldValue, out DiffCounterCommand<int> result)
        {
            if (oldValue != Value)
            {
                result = new DiffCounterCommand<int> { Id = this.Id, Value = modifiersContainer.CurrentValue, PreviousValue = oldValue, MaxValue = modifiersContainer.GetCalculatedValue() };
                return true;
            }

            result = default;
            return false;
        }

        private void UpdatValueWithModifiers(int oldValue, int oldCalculated)
        {
            var percent = oldCalculated > 0 ? oldValue / oldCalculated : 1;
            modifiersContainer.SetCurrentValue(modifiersContainer.GetCalculatedValue() * percent);
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