namespace HECSFramework.Core
{
    public static partial class HMasks
    {
        private static HECSMask dummy = HECSMask.Empty;
        public static ref HECSMask Dummy => ref dummy;

        public static HECSMask GetMask<T> () where T: IComponent
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