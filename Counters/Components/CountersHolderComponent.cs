using HECSFramework.Core;
using System;
using System.Collections.Generic;
using Systems;

namespace Components
{
    [Serializable]
    [RequiredAtContainer(typeof(CountersHolderSystem))]
    [Documentation(Doc.Counters, "This component holds counters from this entity, counters should have processing from CountersHolderSystem")]
    public sealed partial class CountersHolderComponent : BaseComponent, IInitable
    {
        private readonly Dictionary<int, ICounterModifiable<float>> floatCounters = new Dictionary<int, ICounterModifiable<float>>();
        private readonly Dictionary<int, ICounter> counters = new Dictionary<int, ICounter>();

        public ReadOnlyDictionary<int, ICounterModifiable<float>> FloatCounters;
        public ReadOnlyDictionary<int, ICounter> Counters;

        public void Init()
        {
            FloatCounters = new ReadOnlyDictionary<int, ICounterModifiable<float>>(floatCounters);
            Counters = new ReadOnlyDictionary<int, ICounter>(counters);
        }

        public void AddCounter(ICounter counter)
        {
            if (!counters.TryAdd(counter.Id, counter))
                HECSDebug.LogWarning("we alrdy have this counter id" + counter.Id);
        }

        public T GetCounter<T>(int id) where T : ICounter
        {
            if (counters.TryGetValue(id, out var counter))
            {
                if (counter is T needed)
                    return needed;
            }

            return default;
        }

        public bool TryGetCounter<T>(int id, out T getCounter) where T : ICounter
        {
            if (counters.TryGetValue(id, out var counter))
            {
                getCounter = (T)counter;
                return getCounter != null;
            }

            getCounter = default;
            return default;
        }

        public void SetCounter(ICounter<float> counter)
        {
            if (floatCounters.TryGetValue(counter.Id, out var currentCounter))
            {
                currentCounter.SetValue(counter.Value);
            }
        }

        public void SetCounter<T>(T counterComponent) where T : IComponent, ICounter<float>
        {
            if (floatCounters.TryGetValue(counterComponent.Id, out var currentCounter))
            {
                currentCounter.SetValue(counterComponent.Value);
            }
            else
            {
                Owner.AddHecsComponent(counterComponent);
            }
        }

        public void RemoveCounter(ICounter counter)
        {
            counters.Remove(counter.Id);
            floatCounters.Remove(counter.Id);
        }

        public void RemoveCounter(int id)
        {
            counters.Remove(id);
            floatCounters.Remove(id);
        }

        public void AddFloatModifiableCounter(ICounterModifiable<float> counterModifiable)
        {
            if (!floatCounters.TryAdd(counterModifiable.Id, counterModifiable))
            {
                HECSDebug.LogError("we try to add existing ICounterModifiable " + counterModifiable.Id + $" {Owner.ID} {Owner.GUID}");
                return;
            }
        }

        public void RemoveFloatModifiableCounter(ICounterModifiable<float> counterModifiable)
        {
            floatCounters.Remove(counterModifiable.Id);
        }

        public bool TryGetValue<T>(int id, out T value)
        {
            if (counters.TryGetValue(id, out ICounter counter))
            {
                if (counter is ICounter<T> needed)
                {
                    value = needed.Value;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        public void ResetCounters()
        {
            foreach (var c in floatCounters)
            {
                c.Value.Reset();
            }
        }
    }
}