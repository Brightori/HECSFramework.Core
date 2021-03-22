namespace HECSFramework.Core 
{
    public partial struct HECSMask
    {
        public static HECSMask Empty => TypesMap.Empty;

        public int Index;
        public ulong Mask01;

        public static HECSMask operator +(HECSMask l, HECSMask r)
        {
            return new HECSMask
            {
                Mask01 = l.Mask01 | r.Mask01,
            };
        }

        public static HECSMask operator -(HECSMask l, HECSMask r)
        {
            return new HECSMask
            {
                Mask01 = l.Mask01 ^ r.Mask01,
            };
        }

        public bool Contain(ref HECSMask mask)
        {
            return (Mask01 & mask.Mask01) == mask.Mask01;
        }

        public override bool Equals(object obj)
        {
            return obj is HECSMask mask &&
                   Index == mask.Index;
        }

        public override int GetHashCode()
        {
            return -2134847229 + Index.GetHashCode();
        }
    }
}
