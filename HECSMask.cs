using System;
using HECSFramework.Core.Helpers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HECSFramework.Core
{
    [Serializable]
    public partial struct HECSMask : IEquatable<HECSMask>
    {
        public int Index;
        public int TypeHashCode;

        public static HECSMask Empty = new HECSMask { Index = 0, TypeHashCode = 0 };

        public override bool Equals(object obj)
        {
            return obj is HECSMask mask &&
                   Index == mask.Index &&
                   TypeHashCode == mask.TypeHashCode;
        }

        public bool Equals(HECSMask mask)
        {
            return Index == mask.Index &&
                   TypeHashCode == mask.TypeHashCode;
        }
        public override int GetHashCode()
        {
            int hashCode = -523170829;
            hashCode = hashCode * -1521134295 + Index;
            hashCode = hashCode * -1521134295 + TypeHashCode;
            return hashCode;
        }
    }

    [Serializable]
    public struct Filter 
    {
        public int Mask01;
        public int Mask02;
        public int Mask03;
        public int Mask04;
        public int Mask05;
        public int Mask06;
        public int Mask07;

        public int Lenght;

        public int this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index == 0)
                    return Mask01;
                if (index == 1)
                        return Mask02;
                if (index == 2)
                    return Mask03;
                if (index == 3)
                    return Mask04;
                if (index == 4)
                    return Mask05;
                if (index == 5)
                    return Mask06;
                if (index == 6)
                    return Mask07;

                return 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index == 0)
                    Mask01 = value;
                if (index == 1)
                    Mask02 = value;
                if (index == 2)
                    Mask03 = value;
                if (index == 3)
                    Mask04 = value;
                if (index == 4)
                    Mask05 = value;
                if (index == 5)
                    Mask06 = value;
                if (index == 6)
                    Mask07 = value;
            }
        }

        #region Constructors

        public Filter(int mask01) : this()
        {
            Mask01 = mask01;
            Lenght = 1;
            GetHashCode();
        }

        public Filter(int mask01, int mask02) : this()
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Lenght = 2;
            GetHashCode();
        }

        public Filter(int mask01, int mask02, int mask03) : this()
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Mask03 = mask03;
            Lenght = 3;
            GetHashCode();
        } 
        
        public Filter(int mask01, int mask02, int mask03, int mask04) : this()
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Mask03 = mask03;
            Mask04 = mask04;
            Lenght = 4;
            GetHashCode();
        }

        public Filter(int mask01, int mask02, int mask03, int mask04, int mask05) : this()
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Mask03 = mask03;
            Mask04 = mask04;
            Mask05 = mask05;
            Lenght = 5;
            GetHashCode();
        }
        public Filter(int mask01, int mask02, int mask03, int mask04, int mask05, int mask06) : this()
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Mask03 = mask03;
            Mask04 = mask04;
            Mask05 = mask05;
            Mask06 = mask06;
            Lenght = 6;
            GetHashCode();
        }

        public Filter(int mask01, int mask02, int mask03, int mask04, int mask05, int mask06, int mask07) : this()
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Mask03 = mask03;
            Mask04 = mask04;
            Mask05 = mask05;
            Mask06 = mask06;
            Mask07 = mask07;
            Lenght = 7;
            GetHashCode();
        }
        #endregion

        public void Add(int typeIndex)
        {
            var currentIndex = Lenght;

            if (++Lenght <= 7)
            {
                this[currentIndex] = typeIndex;
            }
            else
                Lenght = currentIndex;
        }

        public void AddToHashSet(HashSet<int> typesHashes)
        {
            for (int i = 0; i < Lenght; i++)
            {
                if (this[i] != 0)
                    typesHashes.Add(this[i]);
            }
        }

        public void AddToHashSet(HECSList<int> typesHashes)
        {
            for (int i = 0; i < Lenght; i++)
            {
                if (this[i] != 0)
                    typesHashes.Add(this[i]);
            }
        }


        public static Filter Get<T>() where T: IComponent, new()
        {
            return new Filter(ComponentProvider<T>.TypeIndex);
        }

        public static Filter Get<T,U>() where T : IComponent, new() where U : IComponent, new()
        {
            return new Filter(ComponentProvider<T>.TypeIndex, ComponentProvider<U>.TypeIndex);
        }

        public static Filter Get<T, U, Z>() 
            where T : IComponent, new() 
            where U : IComponent, new()
            where Z : IComponent, new()
        {
            return new Filter(ComponentProvider<T>.TypeIndex, ComponentProvider<U>.TypeIndex, ComponentProvider<Z>.TypeIndex);
        }

        public static Filter Get<T, U, Z, S>()
          where T : IComponent, new()
          where U : IComponent, new()
          where Z : IComponent, new()
          where S : IComponent, new()
        {
            return new Filter(
                ComponentProvider<T>.TypeIndex, 
                ComponentProvider<U>.TypeIndex, 
                ComponentProvider<Z>.TypeIndex,
                ComponentProvider<S>.TypeIndex);
        }

        public static Filter Get<T, U, Z, S, X>()
         where T : IComponent, new()
         where U : IComponent, new()
         where Z : IComponent, new()
         where S : IComponent, new()
         where X : IComponent, new()
        {
            return new Filter(
                ComponentProvider<T>.TypeIndex,
                ComponentProvider<U>.TypeIndex,
                ComponentProvider<Z>.TypeIndex,
                ComponentProvider<X>.TypeIndex,
                ComponentProvider<S>.TypeIndex);
        }

        public static Filter Get<T, U, Z, S, X, L>()
        where T : IComponent, new()
        where U : IComponent, new()
        where Z : IComponent, new()
        where S : IComponent, new()
        where X : IComponent, new()
        where L : IComponent, new()
        {
            return new Filter(
                ComponentProvider<T>.TypeIndex,
                ComponentProvider<U>.TypeIndex,
                ComponentProvider<Z>.TypeIndex,
                ComponentProvider<X>.TypeIndex,
                ComponentProvider<L>.TypeIndex,
                ComponentProvider<S>.TypeIndex);
        }

        public static Filter Get<T, U, Z, S, X, L, F>()
        where T : IComponent, new()
        where U : IComponent, new()
        where Z : IComponent, new()
        where S : IComponent, new()
        where X : IComponent, new()
        where L : IComponent, new()
        where F : IComponent, new()
        {
            return new Filter(
                ComponentProvider<T>.TypeIndex,
                ComponentProvider<U>.TypeIndex,
                ComponentProvider<Z>.TypeIndex,
                ComponentProvider<X>.TypeIndex,
                ComponentProvider<L>.TypeIndex,
                ComponentProvider<F>.TypeIndex,
                ComponentProvider<S>.TypeIndex);
        }

        #region EqualsHashOverrides
        public override bool Equals(object obj)
        {
            return obj is Filter mask && mask.GetHashCode() == GetHashCode();
                   
        }
        public bool Equals(Filter mask)
        {
            return mask.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            int summaryHash = 0;
            summaryHash += Mask01;
            summaryHash += Mask02;
            summaryHash += Mask03;
            summaryHash += Mask04;
            summaryHash += Mask05;
            summaryHash += Mask06;
            summaryHash += Mask07;
            return summaryHash;
        }
        public static Filter operator -(Filter left, HECSMask right)
        {
            left.Lenght--;
            if (left.Mask01.Equals(right))
            {
                left.Mask01 = default;
                return left;
            }
            if (left.Mask02.Equals(right))
            {
                left.Mask02 = default;
                return left;
            }
            if (left.Mask03.Equals(right))
            {
                left.Mask03 = default;
                return left;
            }
            if (left.Mask04.Equals(right))
            {
                left.Mask04 = default;
                return left;
            }
            if (left.Mask05.Equals(right))
            {
                left.Mask05 = default;
                return left;
            }
            if (left.Mask06.Equals(right))
            {
                left.Mask06 = default;
                return left;
            }
            left.Lenght++;
            return left;
        }

        #endregion
    }
}