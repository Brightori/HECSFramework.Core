using HECSFramework.Core;

namespace HECSFramework.Rewards
{
    /// <summary>
    /// этот интерфейс нужен для локальной системы награды, в случае если что то происходит локально
    /// </summary>
    public interface IRewardSystem : IReactCommand<ExecuteReward>
    {
        
    }

    public partial interface IReward
    {
        /// <summary>
        /// здесь мы передаём того - кого надо наградить
        /// </summary>
        /// <param name="entity"> эта ентити цель награды (если нам нужна цель) </param>
        void Award(IEntity entity);
    }

    public abstract partial class RewardBase : IReward
    {
        public abstract void Award(IEntity entity);
    }
}
