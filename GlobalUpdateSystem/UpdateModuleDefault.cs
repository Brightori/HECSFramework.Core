using System.Collections.Generic;

namespace HECSFramework.Core
{
    public sealed class UpdateModuleDefault : BaseUpdatableModule<IUpdatable>, IUpdatable
    {
        public void UpdateLocal()
        {
            ProcessAddRemove();

            var count2 = updateOnEntities.Count;

            for (int i = 0; i < count2; i++)
            {
                if (!updateOnEntities.Data[i].Entity.IsAlive || updateOnEntities.Data[i].Entity.IsPaused) continue;
                updateOnEntities.Data[i].Updatable.UpdateLocal();
            }

            var count = updatables.Count;

            for (int i = 0; i < count; i++)
            {
                IUpdatable fixedUpdatable = updatables.Data[i];
                fixedUpdatable.UpdateLocal();
            }

            ProcessAddRemove();
        }

        protected override void AfterAddOrRemove()
        {
        }
    }
}