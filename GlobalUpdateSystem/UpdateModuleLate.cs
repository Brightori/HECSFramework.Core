using HECSFramework.Core.Helpers;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class UpdateModuleLate : ILateUpdatable, IRegisterUpdate<ILateUpdatable>
    {
        private readonly List<ILateUpdatable> lateUpdatables = new List<ILateUpdatable>(64);
        private int count;

        public void Register(ILateUpdatable updatable, bool add)
        {
            lateUpdatables.AddOrRemoveElement(updatable, add);
        }

        public void UpdateLateLocal()
        {
            count = lateUpdatables.Count;

            for (int i = 0; i < count; i++)
            {
                ILateUpdatable lateUpdatable = lateUpdatables[i];
                lateUpdatable.UpdateLateLocal();
            }
        }
    }
}