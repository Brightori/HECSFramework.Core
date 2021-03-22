using System;

namespace HECSFramework.Core
{
    public class MaskProvider : IMaskProvider
    {
        public bool Contain(ref HECSMask original, ref HECSMask other)
        {
            throw new NotImplementedException();
        }

        public HECSMask GetMinus(HECSMask l, HECSMask r)
        {
            throw new NotImplementedException();
        }

        public HECSMask GetPlus(HECSMask l, HECSMask r)
        {
            throw new NotImplementedException();
        }
    }

    public interface IMaskProvider
    {
        HECSMask GetPlus(HECSMask l, HECSMask r);
        HECSMask GetMinus(HECSMask l, HECSMask r);
        bool Contain(ref HECSMask original, ref HECSMask other);
    }
}