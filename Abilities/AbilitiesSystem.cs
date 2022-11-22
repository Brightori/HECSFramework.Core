using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Documentation(Doc.Abilities, Doc.HECS, "Main system for operating abilities")]
    public sealed partial class AbilitiesSystem : BaseSystem, IAfterEntityInit, IReactCommand<ExecuteAbilityByIDCommand>
    {
        [Required]
        public AbilitiesHolderComponent abilitiesHolderComponent;

        private HECSMask actorContainerIDMask = HMasks.GetMask<ActorContainerID>();

        public void AfterEntityInit()
        {
            foreach (var a in abilitiesHolderComponent.Abilities)
                a.Init();
        }

        public void CommandReact(ExecuteAbilityByIDCommand command)
        {
            foreach (var a in abilitiesHolderComponent.Abilities)
            {
                if (a.TryGetHecsComponent(actorContainerIDMask, out ActorContainerID actorContainerID))
                {
                    if (actorContainerID.ContainerIndex == command.AbilityIndex)
                    {
                        a.Command(new ExecuteAbilityCommand { Enabled = command.Enable, IgnorePredicates = command.IgnorePredicates, Owner = command.Owner, Target = command.Target });
                    }
                }
            }
        }

        public override void InitSystem()
        {
            ClientInit();
        }

        partial void ClientInit();
    }
}