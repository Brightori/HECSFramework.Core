using System;
using System.Collections.Generic;
using Commands;
using HECSFramework.Core;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Counters, "this component is base for all counters components with modifiable values. this component holds modifier container")]
    public abstract partial class ModifiableIntCounterComponent : BaseComponent, IBaseValue<int>, ICounterModifiable<int>, IInitable, IDisposable
    {
        public int Value => modifiableIntCounter.Value;
        public int CalculatedMaxValue => modifiableIntCounter.CalculatedMaxValue;
        public abstract int Id { get; }
        public abstract int SetupValue { get; }
        public int GetBaseValue => SetupValue;

        protected ModifiableIntCounter modifiableIntCounter = new ModifiableIntCounter();
        protected bool isReactive;

        public bool IsReactive { get => isReactive; protected set => isReactive = value; }

        public void Init()
        {
            modifiableIntCounter.Setup(Id, SetupValue);
        }

        public void AddModifier(Guid owner, IModifier<int> modifier)
        {
            var oldValue = Value;
            modifiableIntCounter.AddModifier(owner, modifier);

            if (isReactive)
                Owner.Command(GetDiffCommand(oldValue));
        }

        public void RemoveModifier(Guid owner, IModifier<int> modifier, bool unique = false)
        {
            var oldValue = Value;
            modifiableIntCounter.RemoveModifier(owner, modifier);

            if (isReactive)
                Owner.Command(GetDiffCommand(oldValue));
        }

        public void RemoveModifier(Guid modifierGUID, bool unique = false)
        {
            var oldValue = Value;
            modifiableIntCounter.RemoveModifier(modifierGUID, unique);

            if (isReactive)
                Owner.Command(GetDiffCommand(oldValue));
        }

        public void RemoveModifier(int modifierID, bool unique = false)
        {
            var oldValue = Value;
            modifiableIntCounter.RemoveModifier(modifierID, unique);

            if (isReactive)
                Owner.Command(GetDiffCommand(oldValue));
        }

        public void AddUniqueModifier(Guid owner, IModifier<int> modifier)
        {
            var oldValue = Value;
            modifiableIntCounter.AddUniqueModifier(owner, modifier);

            if (isReactive)
                Owner.Command(GetDiffCommand(oldValue));
        }

        private DiffCounterCommand<int> GetDiffCommand(int oldValue)
        {
            return new DiffCounterCommand<int>
            {
                Id = this.Id,
                Value = modifiableIntCounter.Value,
                PreviousValue = oldValue,
                MaxValue = modifiableIntCounter.CalculatedMaxValue
            };
        }

        public void SetReactive(bool state)
        {
            IsReactive = state;
        }

        public void SetValue(int value)
        {
            var oldValue = Value;
            modifiableIntCounter.SetValue(value);

            if (IsReactive && CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void ChangeValue(int value)
        {
            var oldValue = Value;

            modifiableIntCounter.ChangeValue(value);

            if (IsReactive && CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        private bool CheckModifiedDiff(int oldValue, out DiffCounterCommand<int> result)
        {
            if (oldValue != Value)
            {
                result = new DiffCounterCommand<int> { Id = this.Id, Value = modifiableIntCounter.Value, PreviousValue = oldValue, MaxValue = modifiableIntCounter.CalculatedMaxValue };
                return true;
            }

            result = default;
            return false;
        }

        public void Dispose()
        {
            modifiableIntCounter.Dispose();
        }

        public void Reset()
        {
            modifiableIntCounter.Reset();
        }

        public IEnumerable<IModifier<int>> GetModifiers() => modifiableIntCounter.GetModifiers();

        public void SetupBaseValue(int newBaseValue)
        {
            modifiableIntCounter.Setup(Id, newBaseValue);
        }

        public void SetIsDirty()
        {
            modifiableIntCounter.SetIsDirty();
        }
    }
}