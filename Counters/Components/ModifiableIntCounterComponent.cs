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

        public bool IsReactive { get; protected set; }

        public void Init()
        {
            modifiableIntCounter.Setup(Id, SetupValue);
        }

        public void AddModifier(Guid owner, IModifier<int> modifier) => modifiableIntCounter.AddModifier(owner, modifier);
        public void RemoveModifier(Guid owner, IModifier<int> modifier) => modifiableIntCounter.RemoveModifier(owner, modifier);
        public void AddUniqueModifier(Guid owner, IModifier<int> modifier) => modifiableIntCounter.AddUniqueModifier(owner, modifier);


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
    }
}