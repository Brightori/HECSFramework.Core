namespace HECSFramework.Core
{
    public sealed class UpdateModuleDeltaTime : BaseUpdatableModule<IUpdatableDelta>,  IUpdatableDelta
    {
        public void UpdateLocalDelta(float delta)
        {
            ProcessAddRemove();

            var count2 = updateOnEntities.Count;

            for (int i = 0; i < count2; i++)
            {
                if (!updateOnEntities.Data[i].Entity.IsAlive || updateOnEntities.Data[i].Entity.IsPaused) continue;
                updateOnEntities.Data[i].Updatable.UpdateLocalDelta(delta);
            }

            var count = updatables.Count;

            for (int i = 0; i < count; i++)
            {
                IUpdatableDelta fixedUpdatable = updatables.Data[i];
                fixedUpdatable.UpdateLocalDelta(delta);
            }
        }

        protected override void AfterAddOrRemove()
        {
        }
    }
}