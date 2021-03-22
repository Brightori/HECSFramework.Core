using HECSFramework.Core.Helpers;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class UpdateModuleFixed : IFixedUpdatable, IRegisterUpdate<IFixedUpdatable>
    {
        private readonly List<IFixedUpdatable> fixedUpdatables = new List<IFixedUpdatable>(64);
        private int count;

        public void FixedUpdateLocal()
        {
            count = fixedUpdatables.Count;

            for (int i = 0; i < count; i++)
            {
                IFixedUpdatable fixedUpdatable = fixedUpdatables[i];
                fixedUpdatable.FixedUpdateLocal();
            }
        }

        public void Register(IFixedUpdatable updatable, bool add)
        {
            fixedUpdatables.AddOrRemoveElement(updatable, add);
        }
    }
}