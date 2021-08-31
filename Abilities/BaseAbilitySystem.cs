using Commands;
using Components;

namespace HECSFramework.Core
{
    public abstract class BaseAbilitySystem : BaseSystem, IAbilitySystem, IReactCommand<ExecuteAbilityCommand>
    {
        private HECSMask predicateMask = HMasks.GetMask<PredicatesComponent>();

        public void CommandReact(ExecuteAbilityCommand command)
        {
            if (Owner.TryGetHecsComponent(predicateMask, out PredicatesComponent predicatesComponent))
            {
                if (!predicatesComponent.IsReady(Owner))
                    return;
            }

            Execute(command.Owner, command.Target, command.Enabled);
            Owner.Command(new AbilityWasExecutedCommand());
        }

        public abstract void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}