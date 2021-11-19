using HECSFramework.Core.Helpers;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class UpdateModuleLate : ILateUpdatable, IRegisterUpdate<ILateUpdatable>
    {
        private readonly ConcurrencyList<ILateUpdatable> lateUpdatables = new ConcurrencyList<ILateUpdatable>();
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
                if (lateUpdatable == null) continue;

                lateUpdatable.UpdateLateLocal();
            }
        }
    }
}