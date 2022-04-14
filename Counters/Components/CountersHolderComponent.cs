using System;
using System.Collections.Generic;
using HECSFramework.Core;
using Systems;

namespace Components
{
    [RequiredAtContainer(typeof(CountersHolderSystem))]
    [Serializable][Documentation(Doc.Counters, "This component holds counters from this entity, counters should have processing from CountersHolderSystem")]
    public sealed class CountersHolderComponent : BaseComponent
    {
        public readonly Dictionary<int, ICounterModifiable<float>> FloatCounters = new Dictionary<int, ICounterModifiable<float>>();
        public readonly Dictionary<int, ICounter> Counters = new Dictionary<int, ICounter>();

        public bool TryGetValue<T>(int id, out T value)
        {
            if (Counters.TryGetValue(id, out ICounter counter))
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
            foreach (var c in FloatCounters)
            {
                c.Value.Reset();
            }
        }
    }
}