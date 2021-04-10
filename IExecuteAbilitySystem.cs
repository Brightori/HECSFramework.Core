
namespace HECSFramework.Core
{
    public interface IExecuteAbilitySystem : ISystem
    {
        void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}