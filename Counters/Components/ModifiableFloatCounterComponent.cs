using System;
using System.Collections.Generic;
using Commands;
using HECSFramework.Core;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Counters, "this component is base for all counters components with modifiable values. this component holds modifier container")]
    public abstract partial class ModifiableFloatCounterComponent : BaseComponent, IPoolableComponent, ICounterModifiable<float>, IInitable, IDisposable
    {
        public float Value => modifiableFloatCounter.Value;
        public float CalculatedMaxValue => modifiableFloatCounter.CalculatedMaxValue;
        public abstract int Id { get; }
        public abstract float SetupValue { get; }

        protected ModifiableFloatCounter modifiableFloatCounter = new ModifiableFloatCounter();
        protected bool isReactive;

        public bool IsReactive { get => isReactive; protected set => isReactive = value; }

        public void Init()
        {
            modifiableFloatCounter.Setup(Id, SetupValue);
        }

        public void AddModifier(Guid owner, IModifier<float> modifier) 
        {
            var oldValue = Value;
            modifiableFloatCounter.AddModifier(owner, modifier);

            if (isReactive)
                Owner.Command(GetDiffCommand(oldValue));
        } 

        public void RemoveModifier(Guid owner, IModifier<float> modifier, bool unique = false) 
        {
            var oldValue = Value;
            modifiableFloatCounter.RemoveModifier(owner, modifier, unique);

            if (isReactive)
                Owner.Command(GetDiffCommand(oldValue));
        }

        public void RemoveModifier(Guid modifierGUID, bool unique = false)
        {
            var oldValue = Value;
            modifiableFloatCounter.RemoveModifier(modifierGUID, unique);

            if (isReactive)
                Owner.Command(GetDiffCommand(oldValue));
        }

        public void RemoveModifier(int modifierID, bool unique = false)
        {
            var oldValue = Value;
            modifiableFloatCounter.RemoveModifier(modifierID, unique);

            if (isReactive)
                Owner.Command(GetDiffCommand(oldValue));
        }

        public void AddUniqueModifier(Guid owner, IModifier<float> modifier)
        {
            var oldValue = Value;
            modifiableFloatCounter.AddUniqueModifier(owner, modifier);

            if (isReactive)
                Owner.Command(GetDiffCommand(oldValue));
        }

        private DiffCounterCommand<float> GetDiffCommand(float oldValue)
        {
            return new DiffCounterCommand<float>
            {
                Id = this.Id,
                Value = modifiableFloatCounter.Value,
                PreviousValue = oldValue,
                MaxValue = modifiableFloatCounter.CalculatedMaxValue
            };
        }

        public void SetReactive(bool state)
        {
            IsReactive = state;
        }

        public void SetValue(float value)
        {
            var oldValue = Value;
            modifiableFloatCounter.SetValue(value);

            if (IsReactive && CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        public void ChangeValue(float value)
        {
            var oldValue = Value;

            modifiableFloatCounter.ChangeValue(value);

            if (IsReactive && CheckModifiedDiff(oldValue, out var command))
                Owner.Command(command);
        }

        private bool CheckModifiedDiff(float oldValue, out DiffCounterCommand<float> result)
        {
            if (oldValue != Value)
            {
                result = new DiffCounterCommand<float> { Id = this.Id, Value = modifiableFloatCounter.Value, PreviousValue = oldValue, MaxValue = modifiableFloatCounter.CalculatedMaxValue };
                return true;
            }

            result = default;
            return false;
        }

        public virtual void Dispose()
        {
            modifiableFloatCounter.Dispose();
        }

        public void Reset()
        {
            modifiableFloatCounter.Reset();
        }

        public IEnumerable<IModifier<float>> GetModifiers()
        {
            return modifiableFloatCounter.GetModifiers();
        }

        public void SetIsDirty()
        {
            this.modifiableFloatCounter.SetIsDirty();
        }
    }
}