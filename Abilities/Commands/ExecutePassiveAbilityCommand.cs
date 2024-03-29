﻿using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.Abilities, "Passive abilities are those that are performed only once, when you add them them")]
    public partial struct ExecutePassiveAbilityCommand : ICommand
    {
        public Entity Target;
        public Entity Owner;
        public bool Enabled;
    }
}