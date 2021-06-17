using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace HECSFramework.Core
{
    public class ConcurrencyList<T>
    {
        private T[] data = new T[64];
        public T this[int index] { get => GetT(index); set => Set(value, index); }

        public int Count { get; private set; } = 0;
        public bool IsReadOnly { get; private set; }

        private int locked = 0;
        private int lockFreeStep = 0;

        static readonly T[] _emptyArray = new T[64];

        private ConcurrentQueue<int> freeIndexes = new ConcurrentQueue<int>();

        public void Add(T item)
        {
            Lock();
            IsLockFree();
            Interlocked.Increment(ref lockFreeStep);

            if (IsNeedToResize())
                ResizeArray();

            if (freeIndexes.TryDequeue(out var result))
            {
                data[result] = item;
                UnLock();
                return;
            }

            data[Count] = item;
            Count++;

            UnLock();
        }

        private T GetT(int index)
        {
            if (index > data.Length || index < 0)
                throw new Exception("out of range");

            IsLockFree();
            Interlocked.Increment(ref lockFreeStep);
            return data[index];
        }

        private void Set(T item, int index)
        {
            Lock();
            data[index] = item;
            UnLock();
        }

        private int capacity
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return data.Length;
            }
            set
            {
                if (value != data.Length)
                {
                    if (value > 0)
                    {
                        T[] newItems = new T[value];
                        if (Count > 0)
                        {
                            Array.Copy(data, 0, newItems, 0, Count);
                        }
                        data = newItems;
                    }
                    else
                    {
                        data = _emptyArray;
                    }
                }
            }
        }

        private bool IsLockFree()
        {
            int valueData;
            do
            {
                valueData = lockFreeStep;
            }
            while (!CAS(ref lockFreeStep, valueData + 1, valueData));

            return true;
        }

        private void Lock()
        {
            var spinWait = new SpinWait();

            while (Interlocked.CompareExchange(ref locked, 1, 0) == 0)
            {
                spinWait.SpinOnce();
            }
        }

        private bool CAS(ref int currentValue, int wantedValue, int oldValue)
        {
            return Interlocked.CompareExchange(ref currentValue, wantedValue, oldValue) == oldValue;
        }

        private void UnLock()
        {
            Interlocked.Exchange(ref locked, 0);
        }

        public void Clear()
        {
            Array.Clear(data, 0, Count);
        }

        public bool Contains(T item)
        {
            IsLockFree();

            if ((Object)item == null)
            {
                for (int i = 0; i < Count; i++)
                    if ((Object)data[i] == null)
                        return true;
                return false;
            }
            else
            {
                EqualityComparer<T> c = EqualityComparer<T>.Default;
                for (int i = 0; i < Count; i++)
                {
                    if (c.Equals(data[i], item)) return true;
                }
                return false;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Lock();
            Array.Copy(data, 0, array, arrayIndex, Count);
            UnLock();
        }

        public int IndexOf(T item)
        {
            Lock();
            IsLockFree();

            if (item == null)
            {
                return -1;
            }

            for (int i = 0; i < Count; i++)
            {
                IsLockFree();
                var current = i;

                if (data[current] == null)
                    continue;

                var currentData = data[current];
                Interlocked.Increment(ref lockFreeStep);
                if (currentData.Equals(item))
                {
                    UnLock();
                    return current;
                }
            }
            
            UnLock();
            return -1;
        }

        public void Insert(int index, T item)
        {
            Lock();

            if (IsNeedToResize())
                ResizeArray();

            Array.Copy(data, index, data, index + 1, data.Length - Count);

            data[index] = item;
            Count++;

            UnLock();
        }

        private bool IsNeedToResize()
        {
            if (Count + 2 >= data.Length)
                return true;

            return false;
        }

        private void ResizeArray()
        {
            capacity = data.Length * 2;
        }

        public bool Remove(T item)
        {
            IsLockFree();

            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            Lock();
            Count--;
            data[index] = default(T);
            UnLock();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        [Serializable]
        public struct Enumerator
        {
            private ConcurrencyList<T> list;
            private int index;
            private T current;
            private int lockFreeStep;

            internal Enumerator(ConcurrencyList<T> list)
            {
                this.list = list;
                index = 0;
                current = default(T);
                lockFreeStep = 0;
            }

            private bool IsLockFree()
            {
                int valueData;
                do
                {
                    valueData = lockFreeStep;
                }
                while (!CAS(ref lockFreeStep, valueData + 1, valueData));

                return true;
            }

            private bool CAS(ref int currentValue, int wantedValue, int oldValue)
            {
                return Interlocked.CompareExchange(ref currentValue, wantedValue, oldValue) == oldValue;
            }

            public void Dispose()
            {
                list = null;
            }

            public bool MoveNext()
            {
                ConcurrencyList<T> localList = list;

                if (((uint)index < (uint)localList.Count))
                {
                    current = localList[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = list.Count + 1;
                current = default(T);
                return false;
            }

            public T Current
            {
                get
                {
                    return current;
                }
            }

            void Reset()
            {
                index = 0;
                current = default(T);
            }
        }
    }
}