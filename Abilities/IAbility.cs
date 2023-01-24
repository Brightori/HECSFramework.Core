using Commands;

namespace HECSFramework.Core
{
    public interface IAbility 
    {
        void Execute(Entity owner = null, Entity target = null, bool enable = true);
    }
}