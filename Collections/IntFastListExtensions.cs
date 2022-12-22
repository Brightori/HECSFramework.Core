namespace HECSFramework.Core
{
    using System;
    using System.Runtime.CompilerServices;
    using HECSFramework.Core;

    public static unsafe class IntFastListExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add(this UShortFastList list) {
            var index = list.Count;
            if (++list.Count == list.capacity) {
                ArrayHelpers.Grow(ref list.Data, list.capacity <<= 1);
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Get(this UShortFastList list, in int index) {
            fixed (ushort* d = &list.Data[0]) {
                return *(d + index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this UShortFastList list, in int index, in ushort value) {
            fixed (ushort* d = &list.Data[0]) {
                *(d + index) = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add(this UShortFastList list, in ushort value) {
            var index = list.Count;
            if (++list.Count == list.capacity) {
                ArrayHelpers.Grow(ref list.Data, list.capacity <<= 1);
            }

            fixed (ushort* p = &list.Data[0]) {
                *(p + index) = value;
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddListRange(this UShortFastList list, UShortFastList other) {
            if (other.Count > 0) {
                var newSize = list.Count + other.Count;
                if (newSize > list.capacity) {
                    while (newSize > list.capacity) {
                        list.capacity <<= 1;
                    }

                    ArrayHelpers.Grow(ref list.Data, list.capacity);
                }

                if (list == other) {
                    Array.Copy(list.Data, 0, list.Data, list.Count, list.Count);
                }
                else {
                    Array.Copy(other.Data, 0, list.Data, list.Count, other.Count);
                }

                list.Count += other.Count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(this UShortFastList list, int source, int destination) => list.Data[destination] = list.Data[source];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this UShortFastList list, ushort value) => ArrayHelpers.IndexOfUnsafeUShort(list.Data, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this UShortFastList list, ushort value) => list.RemoveAt(list.IndexOf(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwap(this UShortFastList list, ushort value, out UShortFastList.ResultSwap swap) => list.RemoveAtSwap(list.IndexOf(value), out swap);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAt(this UShortFastList list, int index) {
            --list.Count;
            if (index < list.Count) {
                Array.Copy(list.Data, index + 1, list.Data, index, list.Count - index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveAtSwap(this UShortFastList list, int index, out UShortFastList.ResultSwap swap) {
            if (list.Count-- > 1) {
                swap.oldIndex = list.Count;
                swap.newIndex = index;
                fixed (ushort* d = &list.Data[0]) {
                    *(d + swap.newIndex) = *(d + swap.oldIndex);
                }

                return true;
            }

            swap = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(this UShortFastList list) {
            if (list.Count <= 0) {
                return;
            }

            Array.Clear(list.Data, 0, list.Count);
            list.Count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort(this UShortFastList list) => Array.Sort(list.Data, 0, list.Count, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort(this UShortFastList list, int index, int len) => Array.Sort(list.Data, index, len, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] ToArray(this UShortFastList list) {
            var newArray = new int[list.Count];
            Array.Copy(list.Data, 0, newArray, 0, list.Count);
            return newArray;
        }
    }
}