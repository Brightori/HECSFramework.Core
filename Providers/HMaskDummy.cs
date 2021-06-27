using System.Collections.Generic;

namespace HECSFramework.Core
{
    public static partial class HMasks
    {
        private static HECSMask dummy = HECSMask.Empty;
        public static ref HECSMask Dummy => ref dummy;

        public static HECSMask GetMask<T>() where T : IComponent
        {
            var index = TypesMap.GetHashOfComponentByType<T>();

            if (TypesMap.GetComponentInfo(index, out var mask))
            {
                return mask.ComponentsMask;
            }
            else
                return dummy;
        }

        public static HECSMultiMask GetMMask<T, U>() where T : IComponent where U : IComponent
        {
            return new HECSMultiMask
            {
                A = TypesMap.GetComponentInfo<T>().ComponentsMask,
                B = TypesMap.GetComponentInfo<U>().ComponentsMask,
                C = HECSMask.Empty,
                D = HECSMask.Empty,
            };
        }

        public static HECSMultiMask GetMMask<T, U, Z>() where T : IComponent where U : IComponent where Z : IComponent
        {
            return new HECSMultiMask
            {
                A = TypesMap.GetComponentInfo<T>().ComponentsMask,
                B = TypesMap.GetComponentInfo<U>().ComponentsMask,
                C = TypesMap.GetComponentInfo<Z>().ComponentsMask,
                D = HECSMask.Empty,
            };
        }

        public static HECSMultiMask GetMMask<T, U, Z, X>()
            where T : IComponent where U : IComponent where Z : IComponent where X : IComponent
        {
            return new HECSMultiMask
            {
                A = TypesMap.GetComponentInfo<T>().ComponentsMask,
                B = TypesMap.GetComponentInfo<U>().ComponentsMask,
                C = TypesMap.GetComponentInfo<Z>().ComponentsMask,
                D = TypesMap.GetComponentInfo<X>().ComponentsMask,
            };
        }
    }

    public struct HECSMultiMask
    {
        public HECSMask A;
        public HECSMask B;
        public HECSMask C;
        public HECSMask D;

        public override bool Equals(object obj)
        {
            return obj is HECSMultiMask mask &&
                   EqualityComparer<HECSMask>.Default.Equals(A, mask.A) &&
                   EqualityComparer<HECSMask>.Default.Equals(B, mask.B) &&
                   EqualityComparer<HECSMask>.Default.Equals(C, mask.C) &&
                   EqualityComparer<HECSMask>.Default.Equals(D, mask.D);
        }

        public override int GetHashCode()
        {
            int hashCode = -1408250474;
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            hashCode = hashCode * -1521134295 + C.GetHashCode();
            hashCode = hashCode * -1521134295 + D.GetHashCode();
            return hashCode;
        }
    }
}