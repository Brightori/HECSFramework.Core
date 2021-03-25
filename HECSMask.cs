namespace HECSFramework.Core 
{
    public partial struct HECSMask
    {
        public static HECSMask Empty => TypesMap.MaskProvider.Empty();
        
        public static HECSMask operator +(HECSMask l, HECSMask r) => TypesMap.MaskProvider.GetPlus(l, r);
        public static HECSMask operator -(HECSMask l, HECSMask r) => TypesMap.MaskProvider.GetMinus(l, r);
        public bool Contain(ref HECSMask mask) => TypesMap.MaskProvider.Contains(ref this, ref mask);

        public override bool Equals(object obj) => TypesMap.MaskProvider.GetMaskIsEqual(ref this, obj);

        public override int GetHashCode() => TypesMap.MaskProvider.GetMaskHashCode(ref this);

        public int Index;
        public ulong Mask01;
    }
}