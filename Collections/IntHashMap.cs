///this collection i take from sellecs.morpeh from github
namespace HECSFramework.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    public struct IntHashMapSlot
    {
        public int key;
        public int next;
    }

    [Serializable]
    public sealed partial class IntHashMap<T> : IEnumerable<int>
    {
        public int Count;
        public int Capacity;
        public int CapacityMinusOne;
        public int LastIndex;
        public int FreeIndex;

        public int[] buckets;

        public T[] Data;
        public IntHashMapSlot[] slots;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntHashMap(in int capacity = 0)
        {
            this.LastIndex = 0;
            this.Count = 0;
            this.FreeIndex = -1;

            this.CapacityMinusOne = HashHelpers.GetCapacity(capacity);
            this.Capacity = this.CapacityMinusOne + 1;

            this.buckets = new int[this.Capacity];
            this.slots = new IntHashMapSlot[this.Capacity];
            this.Data = new T[this.Capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            Enumerator e;
            e.hashMap = this;
            e.index = 0;
            e.current = default;
            return e;
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public struct Enumerator : IEnumerator<int>
        {
            public IntHashMap<T> hashMap;

            public int index;
            public int current;

            public bool MoveNext()
            {
                for (; this.index < this.hashMap.LastIndex; ++this.index)
                {
                    ref var slot = ref this.hashMap.slots[this.index];
                    if (slot.key - 1 < 0)
                    {
                        continue;
                    }

                    this.current = this.index;
                    ++this.index;

                    return true;
                }

                this.index = this.hashMap.LastIndex + 1;
                this.current = default;
                return false;
            }

            public int Current => this.current;

            object IEnumerator.Current => this.current;

            void IEnumerator.Reset()
            {
                this.index = 0;
                this.current = default;
            }

            public void Dispose()
            {
            }
        }
    }
}
