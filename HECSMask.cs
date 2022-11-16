using System;
using HECSFramework.Core.Helpers;
using System.Collections.Generic;

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

    public struct FilterMask
    {
        public HECSMask Mask01;
        public HECSMask Mask02;
        public HECSMask Mask03;
        public HECSMask Mask04;
        public HECSMask Mask05;
        public HECSMask Mask06;

        public int Lenght;

        public HECSMask this[int index]
        {
            get
            {
                if (index == 0 && Mask01.TypeHashCode != 0)
                    return Mask01;
                if (index == 1 && Mask02.TypeHashCode != 0)
                        return Mask02;
                if (index == 2 && Mask03.TypeHashCode != 0)
                    return Mask03;
                if (index == 3 && Mask04.TypeHashCode != 0)
                    return Mask04;
                if (index == 4 && Mask05.TypeHashCode != 0)
                    return Mask05;
                if (index == 5 && Mask06.TypeHashCode != 0)
                    return Mask06;

                return HECSMask.Empty;
            }
        }

        public FilterMask(HECSMask mask01) : this()
        {
            Mask01 = mask01;
            Lenght = 1;
        }
        
        public FilterMask(HECSMask mask01, HECSMask mask02) : this()
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Lenght = 2;
        }

        public FilterMask(HECSMask mask01, HECSMask mask02, HECSMask mask03) : this()
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Mask03 = mask03;
            Lenght = 3;
        } 
        
        public FilterMask(HECSMask mask01, HECSMask mask02, HECSMask mask03, HECSMask mask04) : this()
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Mask03 = mask03;
            Mask04 = mask04;
            Lenght = 4;
        }

        public FilterMask(HECSMask mask01, HECSMask mask02, HECSMask mask03, HECSMask mask04, HECSMask mask05) : this()
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Mask03 = mask03;
            Mask04 = mask04;
            Mask05 = mask05;
            Lenght = 5;
        }
        public FilterMask(HECSMask mask01, HECSMask mask02, HECSMask mask03, HECSMask mask04, HECSMask mask05, HECSMask mask06)
        {
            Mask01 = mask01;
            Mask02 = mask02;
            Mask03 = mask03;
            Mask04 = mask04;
            Mask05 = mask05;
            Mask06 = mask06;
            Lenght = 6;
        }

        public override bool Equals(object obj)
        {
            return obj is FilterMask mask &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask01, mask.Mask01) &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask02, mask.Mask02) &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask03, mask.Mask03) &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask04, mask.Mask04) &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask05, mask.Mask05) &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask06, mask.Mask06);
        }
        public bool Equals(FilterMask mask)
        {
            return EqualityComparer<HECSMask>.Default.Equals(Mask01, mask.Mask01) &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask02, mask.Mask02) &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask03, mask.Mask03) &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask04, mask.Mask04) &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask05, mask.Mask05) &&
                   EqualityComparer<HECSMask>.Default.Equals(Mask06, mask.Mask06);
        }

        public override int GetHashCode()
        {
            int hashCode = -830334321;
            hashCode = hashCode * -1521134295 + Mask01.GetHashCode();
            hashCode = hashCode * -1521134295 + Mask02.GetHashCode();
            hashCode = hashCode * -1521134295 + Mask03.GetHashCode();
            hashCode = hashCode * -1521134295 + Mask04.GetHashCode();
            hashCode = hashCode * -1521134295 + Mask05.GetHashCode();
            hashCode = hashCode * -1521134295 + Mask06.GetHashCode();
            return hashCode;
        }
        public static FilterMask operator -(FilterMask left, HECSMask right)
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
    }

    public class HECSMultiMask
    {
        public List<int> CurrentIndexes = new List<int>(8);
        private readonly byte[] Components = new byte[TypesMap.SizeOfComponents];

        public HECSMultiMask() { }

        public HECSMultiMask(HECSMask mask, params HECSMask[] masks)
        {
            CurrentIndexes.Add(mask.Index);
            Components[mask.Index] = 1;

            for (int i = 0; i < masks.Length; i++)
            {
                Components[masks[i].Index] = 1;
                CurrentIndexes.AddUniqueElement(masks[i].Index);
            }
        }

        public HECSMultiMask(FilterMask mask)
        {
            if (mask.Mask01.TypeHashCode != 0)
                AddMask(mask.Mask01.Index);

            if (mask.Mask02.TypeHashCode != 0)
                AddMask(mask.Mask02.Index);

            if (mask.Mask03.TypeHashCode != 0)
                AddMask(mask.Mask03.Index);
            
            if (mask.Mask04.TypeHashCode != 0)
                AddMask(mask.Mask04.Index);
            
            if (mask.Mask05.TypeHashCode != 0)
                AddMask(mask.Mask05.Index);

            if (mask.Mask06.TypeHashCode != 0)
                AddMask(mask.Mask06.Index);
        }

        public void AddMask(int index)
        {
            CurrentIndexes.Add(index);
            Components[index] = 1;
        }

        public void RemoveMask(int index)
        {
            CurrentIndexes.Remove(index);
            Components[index] = 0;
        }

        public bool Contains(HECSMultiMask multiMask)
        {
            var indexes = multiMask.CurrentIndexes;
            var count = indexes.Count;

            if (count == 0)
                return false;

            for (int i = 0; i < count; i++)
            {
                if (Components[indexes[i]] == 0)
                    return false;
            }

            return true;
        }

        public bool Contains(FilterMask multiMask)
        {
            for (int i = 0; i < multiMask.Lenght; i++)
            {
                if (Components[multiMask[i].Index] == 0)
                    return false;
            }

            return true;
        }

        public bool Contains(HECSMask mask)
        {
            return Components[mask.Index] == 1;
        }

        public bool Contains(HECSMask mask1, HECSMask mask2)
        {
            return Contains(mask1) && Contains(mask2);
        }

        public bool ContainsAny(HECSMask mask1, HECSMask mask2)
        {
            return Contains(mask1) || Contains(mask2);
        }

        public bool Contains(HECSMask mask1, HECSMask mask2, HECSMask mask3)
        {
            return Contains(mask1) && Contains(mask2) && Contains(mask3);
        }

        public bool Contains(HECSMask mask1, HECSMask mask2, HECSMask mask3, HECSMask mask4)
        {
            return Contains(mask1, mask2) && Contains(mask3, mask4);
        }

        public bool Contains(HECSMask mask, params HECSMask[] moreMasks)
        {
            return Contains(mask) && Contains(moreMasks);
        }

        private bool Contains(HECSMask[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (Components[array[i].Index] == 0)
                    return false;
            }

            return true;
        }

        public bool ContainsAny(FilterMask mask)
        {
            for (int i = 0; i < mask.Lenght; i++)
            {
                var currentMask = mask[i];

                if (Contains(currentMask))
                    return true;
            }

            return false;
        }

        internal bool ContainsAny(HECSMultiMask mask)
        {
            foreach (var index in mask.CurrentIndexes)
            {
                if (Components[index] == 1)
                    return true;
            }

            return false;
        }
    }
}