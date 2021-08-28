using Commands;
using Components;

namespace HECSFramework.Core
{
    public abstract class ActiveAbilitySystem : BaseSystem, IActiveAbilitySystem
    {
        private HECSMask predicateMask = HMasks.GetMask<PredicatesComponent>();

        public void CommandReact(ExecuteActiveAbilityCommand command)
        {
            if (Owner.TryGetHecsComponent(predicateMask, out PredicatesComponent predicatesComponent))
            {
                if (!predicatesComponent.IsReady(Owner))
                    return;
            }

            Execute(command.Owner, command.Target, command.Enabled);
        }

        public abstract void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }

    public abstract class PassiveAbilitySystem : BaseSystem, IPassiveAbilitySystem
    {
        private HECSMask predicateMask = HMasks.GetMask<PredicatesComponent>();

        public void CommandReact(ExecutePassiveAbilityCommand command)
        {
            if (Owner.TryGetHecsComponent(predicateMask, out PredicatesComponent predicatesComponent))
            {
                if (!predicatesComponent.IsReady(Owner))
                    return;
            }

            Execute(command.Owner, command.Target, command.Enabled);
        }

        public abstract void Execute(IEntity owner = null, IEntity target = null, bool enable = true);
    }
}