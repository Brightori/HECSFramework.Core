using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace HECSFramework.Core
{
    [Serializable]
    public sealed partial class HECSList<T> : IEnumerable<T>
    {
        private class SortComparer : IComparer<T>
        {
            public Comparison<T> Comparer;

            public int Compare(T x, T y)
            {
                return Comparer.Invoke(x, y);
            }
        }


        public T[] Data;
        private int length;
        private int capacity;

        public int Count => length;

        
        public ref T this[int index] 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return ref Data[index];
            }
        }

        public EqualityComparer<T> Comparer;
        private SortComparer sortComparer;

        #region Constructor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HECSList()
        {
            this.capacity = 4;
            this.Data = new T[this.capacity];
            this.length = 0;

            this.Comparer = EqualityComparer<T>.Default;
            this.sortComparer = new SortComparer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HECSList(int capacity)
        {
            this.capacity = capacity;
            this.Data = new T[this.capacity];
            this.length = 0;

            this.Comparer = EqualityComparer<T>.Default;
            this.sortComparer = new SortComparer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HECSList(HECSList<T> other)
        {
            this.capacity = other.capacity;
            this.Data = new T[this.capacity];
            this.length = other.length;
            Array.Copy(other.Data, 0, this.Data, 0, this.length);

            this.Comparer = other.Comparer;
            this.sortComparer = new SortComparer();
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CAS(ref int currentValue, int wantedValue, int oldValue)
        {
            return Interlocked.CompareExchange(ref currentValue, wantedValue, oldValue) == oldValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(T value)
        {
            var index = length;
            if (++length == capacity)
            {
                ArrayHelpers.Grow(ref Data, capacity <<= 1);
            }

            Data[index] = value;
            return index;
        }

        public int AddToIndex(T value, int neededIndex)
        {
            if (neededIndex >= capacity)
            {
                ArrayHelpers.Grow(ref Data, neededIndex * 2);
                capacity = neededIndex * 2;
            }

            Data[neededIndex] = value;

            if (neededIndex >= Count)
                length = neededIndex + 1;

            return neededIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Comparer.Equals(Data[i], item)) return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddListRange(HECSList<T> other)
        {
            if (other.length > 0)
            {
                var newSize = length + other.length;
                if (newSize > capacity)
                {
                    while (newSize > capacity)
                    {
                        capacity <<= 1;
                    }

                    ArrayHelpers.Grow(ref Data, capacity);
                }

                if (this == other)
                {
                    Array.Copy(Data, 0, Data, length, length);
                }
                else
                {
                    Array.Copy(other.Data, 0, Data, length, other.length);
                }

                length += other.length;
            }
        }

        public void Sort(Comparison<T> comparison)
        {
            sortComparer.Comparer = comparison;
            Array.Sort(Data, 0, length, sortComparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Swap(int source, int destination) => Data[destination] = Data[source];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T value) => ArrayHelpers.IndexOf(Data, value, Comparer, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T value)
        {
            var index = IndexOf(value);

            if (index > -1)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveSwap(T value) => RemoveAtSwap(IndexOf(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            --length;
            if (index < length)
            {
                Array.Copy(Data, index + 1, Data, index, length - index);
            }

            Data[length] = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAtSwap(int index)
        {
            if (index < 0)
            {
                return false;
            }

            if (length-- > 1)
            {
                int oldIndex = length;
                int newIndex = index;

                Data[newIndex] = Data[oldIndex];
                Data[oldIndex] = default;
                return true;
            }
            else
            {
                Data[0] = default;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAtSwap(int index, out T newValue)
        {
            if (length-- > 1)
            {
                var oldIndex = length;
                newValue = Data[index] = Data[oldIndex];
                return true;
            }

            newValue = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (length <= 0)
            {
                return;
            }

            Array.Clear(Data, 0, length);
            length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFast()
        {
            length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            var newArray = new T[length];
            Array.Copy(Data, 0, newArray, 0, length);
            return newArray;
        }
        #endregion

        #region Enumerator

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            Enumerator e;
            e.list = this;
            e.current = default;
            e.index = 0;
            return e;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public struct ResultSwap
        {
            public int oldIndex;
            public int newIndex;
        }

        public struct Enumerator : IEnumerator<T>
        {
            public HECSList<T> list;

            public T current;
            public int index;

            public bool MoveNext()
            {
                if (this.index >= this.list.length)
                {
                    return false;
                }

                this.current = this.list.Data[this.index++];
                return true;
            }

            public void Reset()
            {
                this.index = 0;
                this.current = default;
            }

            public  T Current => this.current;
            object IEnumerator.Current => this.current;

            public void Dispose()
            {
            }
        }
        #endregion
    }
}