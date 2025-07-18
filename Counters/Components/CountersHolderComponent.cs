﻿using System;
using System.Collections.Generic;
using HECSFramework.Core;
using Systems;

namespace Components
{
    [Serializable]
    [RequiredAtContainer(typeof(CountersHolderSystem))]
    [Documentation(Doc.Counters, "This component holds counters from this entity, counters should have processing from CountersHolderSystem")]
    public sealed partial class CountersHolderComponent : BaseComponent, IDisposable, IPoolableComponent
    {
        //this collection holds all counters
        private readonly Dictionary<int, ICounter> counters = new Dictionary<int, ICounter>();
        public  ReadOnlyDictionary<int, ICounter> Counters;

        public void AddCounter(ICounter counter)
        {
            counters.TryAdd(counter.Id, counter);
        }

        public float GetFloatValue(int counterID)
        {
            if (counters.TryGetValue(counterID, out var counter))
            {
                if (counter is ICounter<float> floatCounter)
                {
                    return floatCounter.Value;
                }
                else if (counter is ICounter<int> intCounter)
                    return intCounter.Value;
            }

            return 0;
        }

        public int GetIntValue(int counterID)
        {
            if (counters.TryGetValue(counterID, out var counter))
            {
                if (counter is ICounter<int> floatCounter)
                {
                    return floatCounter.Value;
                }
                else if (counter is ICounter<float> intCounter)
                    return (int)intCounter.Value;
            }
                
            return 0;
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

        public void SetOrAddCounter<T>(ICounter<T> counter) where T: struct
        {
            if (TryGetCounter<ICounter<T>>(counter.Id, out var currentCounter))
                currentCounter.SetValue(counter.Value);
            else
                AddCounter(counter);
        }

        public ICounter<float> GetOrAddFloatCounter(int index)  
        {
            if (TryGetCounter<ICounter<float>>( index, out var counter))
                return counter;

            var newCounter = new DefaultFloatCounter ( index );

            AddCounter(newCounter);
            return newCounter;
        }

        public ICounter<int> GetOrAddIntCounter(int index)
        {
            if (TryGetCounter<ICounter<int>>(index, out var counter))
                return counter;

            var newCounter = new DefaultIntCounter (index);

            AddCounter(newCounter);
            return newCounter;
        }


        public void SetOrAddCounter(ICounter counter)
        {
            if (counters.ContainsKey(counter.Id))
            {
                switch (counter)
                {
                    case ICounter<float> floatCounter:
                        (counters[counter.Id] as ICounter<float>).SetValue(floatCounter.Value);
                        break;
                    case ICounter<int> intCounter:
                        (counters[counter.Id] as ICounter<int>).SetValue(intCounter.Value);
                        break;
                }
            }
            else
            {
                AddCounter(counter);
            }
        }

        public void RemoveCounter(ICounter counter)
        {
            RemoveCounter(counter.Id);
        }

        public void RemoveCounter(int id)
{
            counters.Remove(id);
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
            foreach (var c in counters)
            {
                if (c.Value is IResetModifiers reset)
                    reset.Reset();
            }
        }

        public override void Init()
        {
            Counters = new ReadOnlyDictionary<int, ICounter>(counters);
        }

        public void Dispose()
        {
            counters.Clear();
        }
    }
}