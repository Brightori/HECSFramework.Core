namespace HECSFramework.Core
{
    public static partial class HMasks
    {
        private static HECSMask dummy = new HECSMask { Index = 999999, TypeHashCode = - 9999999 };
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
    }
}