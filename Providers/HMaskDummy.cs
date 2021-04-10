namespace HECSFramework.Core
{
    public static partial class HMasks
    {
        private static HECSMask dummy = HECSMask.Empty;
        public static ref HECSMask Dummy => ref dummy;
    }
}