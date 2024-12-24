namespace HECSFramework.Core
{
    public class PriorityLateUpdateModule : BaseUpdatableModule<IPriorityLateUpdatable>, IUpdatable
    {
        public void UpdateLocal()
        {
            ProcessAddRemove();
            UpdateWithOwners();

            var count = updatables.Count;
            for (int i = 0; i < count; i++)
            {
                IPriorityLateUpdatable updatable = updatables.Data[i];
                updatable.PriorityLateUpdateLocal();
            }
        }

        private void UpdateWithOwners()
        {
            var count = updateOnEntities.Count;

            for (int i = 0; i < count; i++)
            {
                if (!updateOnEntities.Data[i].Entity.IsAlive || updateOnEntities.Data[i].Entity.IsPaused) continue;
                updateOnEntities.Data[i].Updatable.PriorityLateUpdateLocal();
            }
        }

        protected override void AfterAddOrRemove()
        {
            updatables.Sort(SortPriorities);
            updateOnEntities.Sort(SortWithOwners);
        }

        private int SortWithOwners(UpdateWithOwnerContainer left, UpdateWithOwnerContainer right)
        {
            if (left.Updatable.Priority == right.Updatable.Priority)
                return 0;

            if (left.Updatable.Priority < right.Updatable.Priority)
                return -1;
            else
                return 1;
        }

        private int SortPriorities(IPriorityLateUpdatable left, IPriorityLateUpdatable right)
        {
            if (left.Priority == right.Priority)
                return 0;

            if (left.Priority < right.Priority)
                return -1;
            else
                return 1;
        }
    }
}
