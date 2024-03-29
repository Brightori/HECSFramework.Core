﻿namespace HECSFramework.Core
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static partial class ArrayHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Grow<T>(ref T[] array, int newSize)
        {
            var newArray = new T[newSize];
            Array.Copy(array, 0, newArray, 0, array.Length);
            array = newArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(T[] array, T value, EqualityComparer<T> comparer, int calculatedLenght)
        {
            for (int i = 0, length = calculatedLenght; i < length; ++i)
            {
                if (comparer.Equals(array[i], value))
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int IndexOfUnsafeInt(int[] array, int value)
        {
            fixed (int* arr = &array[0])
            {
                var i = 0;
                for (int* current = arr, length = arr + array.Length; current < length; ++current)
                {
                    if (*current == value)
                    {
                        return i;
                    }

                    ++i;
                }
            }


            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int IndexOfUnsafeUShort(ushort[] array, ushort value)
        {
            fixed (ushort* arr = &array[0])
            {
                var i = 0;
                for (ushort* current = arr, length = arr + array.Length; current < length; ++current)
                {
                    if (*current == value)
                    {
                        return i;
                    }

                    ++i;
                }
            }


            return -1;
        }
    }
}