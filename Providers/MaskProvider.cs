namespace HECSFramework.Core
{
    public delegate bool ContainsMask(ref HECSMask original, ref HECSMask other);
    public delegate HECSMask MaskOperation(HECSMask left, HECSMask right);
    public delegate HECSMask GetMask();

    public partial class MaskProvider : IMaskProvider
    {
        public MaskOperation GetPlus { get; private set; }
        public MaskOperation GetMinus { get; private set; }
        public ContainsMask Contains { get; private set; }
        public GetMask Empty { get; private set; }
    }

    public partial class MaskProvider
    {
        public MaskProvider()
        {
            Contains = ContainsFunc;
            GetMinus = GetMinusFunc;
        }

        public HECSMask GetMinusFunc(HECSMask left, HECSMask right)
        {
            return default;
        }

        private bool ContainsFunc(ref HECSMask original, ref HECSMask other)
        {
            return default;
        }
    }


    public interface IMaskProvider
    {
        GetMask Empty { get; }
        MaskOperation GetPlus { get; }
        MaskOperation GetMinus { get; }
        ContainsMask Contains { get; }
    }
}