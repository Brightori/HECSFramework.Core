using System;
using System.Collections.Generic;
using HECSFramework.Core;
using Systems;

namespace Components
{
    [RequiredAtContainer(typeof(CountersHolderSystem))]
    [Serializable]
    [Documentation(Doc.Counters, "This component holds counters from this entity, counters should have processing from CountersHolderSystem")]
    public sealed class CountersHolderComponent : BaseComponent, IInitable
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
            {
                HECSDebug.LogWarning("we alrdy have this counter id" + counter.Id);
                return;
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