using Commands;
using HECSFramework.Core;

namespace HECSFramework.Rewards
{
   [Documentation(Doc.Rewards, "base system for rewards")]
    public interface IRewardSystem : IReactCommand<ExecuteReward>
    {
        
    }
}
