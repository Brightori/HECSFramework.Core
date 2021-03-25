namespace HECSFramework.Core
{
    public delegate bool ContainsMask(ref HECSMask original, ref HECSMask other);
    public delegate HECSMask MaskOperation(HECSMask left, HECSMask right);
    public delegate HECSMask GetMask();
    public delegate bool GetMaskIsEqual(ref HECSMask mask, object other);
    public delegate int GetMaskHashCode(ref HECSMask mask);

    public partial class MaskProvider : IMaskProvider
    {
        public MaskOperation GetPlus { get; private set; }
        public MaskOperation GetMinus { get; private set; }
        public ContainsMask Contains { get; private set; }
        public GetMask Empty { get; private set; }
        public GetMaskIsEqual GetMaskIsEqual { get; private set; }
        public GetMaskHashCode GetMaskHashCode { get; private set; }
    }

    public partial class MaskProvider
    {
        //public MaskProvider()
        //{
        //    GetPlus = GetPlusFunc;
        //    GetMinus = GetMinusFunc;
        //    Contains = ContainsFunc;
        //    Empty = GetEmptyMaskFunc;

        //    GetMaskIsEqual = GetEqualityOfMasksFunc;
        //    GetMaskHashCode = GetHashCodeFunc;
        //}

        //public HECSMask GetEmptyMaskFunc()
        //{
        //    return default;
        //}

        //public bool GetEqualityOfMasksFunc(ref HECSMask mask, object other)
        //{
        //    return other is HECSMask otherMask  && mask.Mask01 == otherMask.Mask01;
        //}

        //public HECSMask GetMinusFunc(HECSMask left, HECSMask right)
        //{
        //    return default;
        //}   
        
        //public HECSMask GetPlusFunc(HECSMask left, HECSMask right)
        //{
        //    return default;
        //}

        //private bool ContainsFunc(ref HECSMask original, ref HECSMask other)
        //{
        //    return (original.Mask01 & other.Mask01) == other.Mask01;
        //}

        //public int GetHashCodeFunc(ref HECSMask mask)
        //{
        //    unchecked
        //    {
        //        int hash = -2134847229;
        //        hash += (-1 * (int)mask.Mask01);

        //        return hash;
        //    }
        //}
    }


    public interface IMaskProvider
    {
        GetMask Empty { get; }
        MaskOperation GetPlus { get; }
        MaskOperation GetMinus { get; }
        ContainsMask Contains { get; }
        GetMaskIsEqual GetMaskIsEqual { get; }
        GetMaskHashCode GetMaskHashCode { get; }
    }
}