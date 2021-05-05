
using Commands;

namespace HECSFramework.Core
{
    public abstract class ActiveAbilitySystem : BaseSystem, IActiveAbilitySystem
    {
        public void CommandReact(ExecuteActiveAbilityCommand command) => Execute(command.Owner, command.Target, command.Enabled);
        public abstract void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}