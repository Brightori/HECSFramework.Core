namespace HECSFramework.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    public sealed unsafe partial class UShortFastList : IEnumerable<ushort> {
        public int Count;
        public int capacity;

        public ushort[] Data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UShortFastList() {
            this.capacity = 3;
            this.Data     = new ushort[this.capacity];
            this.Count   = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UShortFastList(int capacity) {
            this.capacity = capacity > 0 ? capacity : 8;
            this.Data     = new ushort[this.capacity];
            this.Count   = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UShortFastList(UShortFastList other) {
            this.capacity = other.capacity;
            this.Data     = new ushort[this.capacity];
            this.Count   = other.Count;
            Array.Copy(other.Data, 0, this.Data, 0, this.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.intFastList = this;
            e.current     = default;
            e.index       = 0;
            return e;
        }

        IEnumerator<ushort> IEnumerable<ushort>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public struct ResultSwap {
            public int oldIndex;
            public int newIndex;
        }

      
        public struct Enumerator : IEnumerator<ushort> {
            public UShortFastList intFastList;

            public ushort current;
            public int index;

            public bool MoveNext() {
                if (this.index >= this.intFastList.Count) {
                    return false;
                }

                fixed (ushort* d = &this.intFastList.Data[0]) {
                    this.current = *(d + this.index++);
                }

                return true;
            }

            public void Reset() {
                this.index   = 0;
                this.current = default;
            }

            public ushort Current => this.current;
            object IEnumerator.Current => this.current;

            public void Dispose() {
            }
        }
    }
}