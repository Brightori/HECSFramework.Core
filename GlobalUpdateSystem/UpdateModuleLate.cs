namespace HECSFramework.Core
{
    public class UpdateModuleLate : BaseUpdatableModule<ILateUpdatable>, ILateUpdatable
    {
        public void UpdateLateLocal()
        {
            ProcessAddRemove();

            var count2 = updateOnEntities.Count;

            for (int i = 0; i < count2; i++)
            {
                if (!updateOnEntities.Data[i].Entity.IsAlive || updateOnEntities.Data[i].Entity.IsPaused) continue;
                updateOnEntities.Data[i].Updatable.UpdateLateLocal();
            }

            var count = updatables.Count;

            for (int i = 0; i < count; i++)
            {
                ILateUpdatable fixedUpdatable = updatables.Data[i];
                fixedUpdatable.UpdateLateLocal();
            }
        }

        protected override void AfterAddOrRemove()
        {
        }
    }
}