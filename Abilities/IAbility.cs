using Commands;

namespace HECSFramework.Core
{
    public interface IAbility 
    {
        void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}