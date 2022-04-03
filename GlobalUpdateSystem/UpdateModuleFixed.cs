namespace HECSFramework.Core
{
    public sealed class UpdateModuleFixed : BaseUpdatableModule<IFixedUpdatable>
    {
        public void FixedUpdateLocal()
        {
            ProcessAddRemove();

            var count2 = updateOnEntities.Count;

            for (int i = 0; i < count2; i++)
            {
                if (!updateOnEntities[i].Entity.IsAlive || updateOnEntities[i].Entity.IsPaused) continue;
                updateOnEntities[i].Updatable.FixedUpdateLocal();
            }

            var count = updatables.Count;

            for (int i = 0; i < count; i++)
            {
                IFixedUpdatable fixedUpdatable = updatables[i];
                fixedUpdatable.FixedUpdateLocal();
            }
        }

        protected override void AfterAddOrRemove()
        {
        }
    }
}