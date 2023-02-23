//Morpeh.Collections 
namespace HECSFramework.Core
{
    using System;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    public static partial class IntHashMapExtensions 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Add<T>(this IntHashMap<T> hashMap, in int key, in T value, out int slotIndex) {
            var rem = key & hashMap.CapacityMinusOne;

            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = hashMap.slots[i].next) {
                if (hashMap.slots[i].key - 1 == key) {
                    slotIndex = -1;
                    return false;
                }
            }

            if (hashMap.FreeIndex >= 0) {
                slotIndex         = hashMap.FreeIndex;
                hashMap.FreeIndex = hashMap.slots[slotIndex].next;
            }
            else {
                if (hashMap.LastIndex == hashMap.Capacity) {
                    var newCapacityMinusOne = HashHelpers.ExpandCapacity(hashMap.Count);
                    var newCapacity         = newCapacityMinusOne + 1;

                    ArrayHelpers.Grow(ref hashMap.slots, newCapacity);
                    ArrayHelpers.Grow(ref hashMap.Data, newCapacity);

                    var newBuckets = new int[newCapacity];

                    for (int i = 0, len = hashMap.LastIndex; i < len; ++i) {
                        ref var slot = ref hashMap.slots[i];

                        var newResizeIndex = (slot.key - 1) & newCapacityMinusOne;
                        slot.next = newBuckets[newResizeIndex] - 1;

                        newBuckets[newResizeIndex] = i + 1;
                    }

                    hashMap.buckets          = newBuckets;
                    hashMap.Capacity         = newCapacity;
                    hashMap.CapacityMinusOne = newCapacityMinusOne;

                    rem = key & hashMap.CapacityMinusOne;
                }

                slotIndex = hashMap.LastIndex;
                ++hashMap.LastIndex;
            }

            ref var newSlot = ref hashMap.slots[slotIndex];

            newSlot.key  = key + 1;
            newSlot.next = hashMap.buckets[rem] - 1;

            hashMap.Data[slotIndex] = value;

            hashMap.buckets[rem] = slotIndex + 1;

            ++hashMap.Count;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Set<T>(this IntHashMap<T> hashMap, in int key, in T value, out int slotIndex) {
            var rem = key & hashMap.CapacityMinusOne;

            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = hashMap.slots[i].next) {
                if (hashMap.slots[i].key - 1 == key) {
                    hashMap.Data[i] = value;
                    slotIndex       = i;
                    return false;
                }
            }

            if (hashMap.FreeIndex >= 0) {
                slotIndex         = hashMap.FreeIndex;
                hashMap.FreeIndex = hashMap.slots[slotIndex].next;
            }
            else {
                if (hashMap.LastIndex == hashMap.Capacity) {
                    var newCapacityMinusOne = HashHelpers.ExpandCapacity(hashMap.Count);
                    var newCapacity         = newCapacityMinusOne + 1;

                    ArrayHelpers.Grow(ref hashMap.slots, newCapacity);
                    ArrayHelpers.Grow(ref hashMap.Data, newCapacity);

                    var newBuckets = new int[newCapacity];

                    for (int i = 0, len = hashMap.LastIndex; i < len; ++i) {
                        ref var slot           = ref hashMap.slots[i];
                        var     newResizeIndex = (slot.key - 1) & newCapacityMinusOne;
                        slot.next = newBuckets[newResizeIndex] - 1;

                        newBuckets[newResizeIndex] = i + 1;
                    }

                    hashMap.buckets          = newBuckets;
                    hashMap.Capacity         = newCapacity;
                    hashMap.CapacityMinusOne = newCapacityMinusOne;

                    rem = key & hashMap.CapacityMinusOne;
                }

                slotIndex = hashMap.LastIndex;
                ++hashMap.LastIndex;
            }

            ref var newSlot = ref hashMap.slots[slotIndex];

            newSlot.key  = key + 1;
            newSlot.next = hashMap.buckets[rem] - 1;

            hashMap.Data[slotIndex] = value;

            hashMap.buckets[rem] = slotIndex + 1;

            ++hashMap.Count;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Remove<T>(this IntHashMap<T> hashMap, in int key, out T lastValue) {
            var rem = key & hashMap.CapacityMinusOne;

            int next;
            int num = -1;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    if (num < 0) {
                        hashMap.buckets[rem] = slot.next + 1;
                    }
                    else {
                        hashMap.slots[num].next = slot.next;
                    }

                    lastValue       = hashMap.Data[i];
                    hashMap.Data[i] = default;

                    slot.key  = -1;
                    slot.next = hashMap.FreeIndex;

                    --hashMap.Count;
                    if (hashMap.Count == 0) {
                        hashMap.LastIndex = 0;
                        hashMap.FreeIndex = -1;
                    }
                    else {
                        hashMap.FreeIndex = i;
                    }

                    return true;
                }

                next = slot.next;
                num  = i;
            }

            lastValue = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this IntHashMap<T> hashMap, in int key) {
            var rem = key & hashMap.CapacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    return true;
                }

                next = slot.next;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this IntHashMap<T> hashMap, in int key, out T value) {
            var rem = key & hashMap.CapacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    value = hashMap.Data[i];
                    return true;
                }

                next = slot.next;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueByKey<T>(this IntHashMap<T> hashMap, in int key) {
            var rem = key & hashMap.CapacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    return hashMap.Data[i];
                }

                next = slot.next;
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T TryGetValueRefByKey<T>(this IntHashMap<T> hashMap, in int key, out bool exist) 
        {
            var rem = key & hashMap.CapacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    exist = true;
                    return ref hashMap.Data[i];
                }

                next = slot.next;
            }

            exist = false;
            return ref hashMap.Data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetValueRefByKey<T>(this IntHashMap<T> hashMap, in int key) {
            var rem = key & hashMap.CapacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    return ref hashMap.Data[i];
                }

                next = slot.next;
            }

            return ref hashMap.Data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueByIndex<T>(this IntHashMap<T> hashMap, in int index) => hashMap.Data[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetKeyByIndex<T>(this IntHashMap<T> hashMap, in int index) => hashMap.slots[index].key - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TryGetIndex<T>(this IntHashMap<T> hashMap, in int key) {
            var rem = key & hashMap.CapacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    return i;
                }

                next = slot.next;
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this IntHashMap<T> hashMap, T[] array) {
            int num = 0;
            for (int i = 0, li = hashMap.LastIndex; i < li && num < hashMap.Count; ++i) {
                if (hashMap.slots[i].key - 1 < 0) {
                    continue;
                }

                array[num] = hashMap.Data[i];
                ++num;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this IntHashMap<T> hashMap) {
            if (hashMap.LastIndex <= 0) {
                return;
            }

            Array.Clear(hashMap.slots, 0, hashMap.LastIndex);
            Array.Clear(hashMap.buckets, 0, hashMap.Capacity);
            Array.Clear(hashMap.Data, 0, hashMap.Capacity);

            hashMap.LastIndex = 0;
            hashMap.Count    = 0;
            hashMap.FreeIndex = -1;
        }
    }
}