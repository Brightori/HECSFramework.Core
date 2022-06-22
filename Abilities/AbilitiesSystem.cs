using Components;
using HECSFramework.Core;

namespace Systems
{
    [Documentation(Doc.Abilities, Doc.HECS, "Main system for operating abilities")]
    public sealed partial class AbilitiesSystem : BaseSystem, IAfterEntityInit
    {
        [Required]
        public AbilitiesHolderComponent abilitiesHolderComponent;

        public void AfterEntityInit()
        {
            foreach (var a in abilitiesHolderComponent.Abilities)
                a.Init();
        }

        public override void InitSystem()
        {
        }
    }
}