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
        private readonly Dictionary<(int id, int subId), ICounterModifiable<float>> complexFloatCounters = new Dictionary<(int id, int subId), ICounterModifiable<float>>();
        private readonly Dictionary<int, ICounter> counters = new Dictionary<int, ICounter>();
        private readonly Dictionary<(int id, int subId), ICounter> complexCounters = new Dictionary<(int id, int subId), ICounter>();

        public ReadOnlyDictionary<int, ICounterModifiable<float>> FloatCounters;
        public ReadOnlyDictionary<(int id, int subId), ICounterModifiable<float>> ComplexFloatCounters;
        public ReadOnlyDictionary<int, ICounter> Counters;
        public ReadOnlyDictionary<(int id, int subId), ICounter> ComplexCounters;

        public void Init()
        {
            FloatCounters = new ReadOnlyDictionary<int, ICounterModifiable<float>>(floatCounters);
            Counters = new ReadOnlyDictionary<int, ICounter>(counters);
            ComplexFloatCounters = new ReadOnlyDictionary<(int main, int subId), ICounterModifiable<float>>(complexFloatCounters);
            ComplexCounters = new ReadOnlyDictionary<(int main, int subId), ICounter>(complexCounters);
        }

        public void AddFloatModifiableCounter(ICounterModifiable<float> counterModifiable)
        {
            if (floatCounters.ContainsKey(counterModifiable.Id))
            {
                HECSDebug.LogError("we try to add existing ICounterModifiable " + counterModifiable.Id + $" {Owner.ID} {Owner.GUID}");
                return;
            }

            floatCounters.Add(counterModifiable.Id, counterModifiable);
        }

        public void AddComplexFloatModifiableCounter((int key, int subKey) id, ICounterModifiable<float> counterModifiable)
        {
            if (complexFloatCounters.ContainsKey(id))
            {
                HECSDebug.LogError("we try to add existing ICounterModifiable " + counterModifiable.Id + $" {Owner.ID} {Owner.GUID}");
                return;
            }

            counters.Add(counterModifiable.Id, counterModifiable);
        }

        public void AddCounter(ICounter counter)
        {
            if (counter is ISubCounter subCounter)
            {
                if (complexCounters.ContainsKey((subCounter.Id, subCounter.SubId)))
                {
                    HECSDebug.LogError("we try to add existing complext counter " + counter.Id + $" {Owner.ID} {Owner.GUID}");
                    return;
                }

                complexCounters.Add((subCounter.Id, subCounter.SubId), counter);
            }
            else
            {
                if (counters.ContainsKey(counter.Id))
                {
                    HECSDebug.LogError("we try to add existing counter " + counter.Id + $" {Owner.ID} {Owner.GUID}");
                    return;
                }

                counters.Add(counter.Id, counter);
            }
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

        public bool TryGetValue<T>((int id, int subId) id, out T value)
        {
            if (complexFloatCounters.TryGetValue(id, out ICounterModifiable<float> counter))
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