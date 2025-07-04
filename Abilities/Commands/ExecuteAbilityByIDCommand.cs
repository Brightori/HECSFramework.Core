﻿using HECSFramework.Core;

namespace Commands
{
    public struct ExecuteAbilityByIDCommand : ICommand
    {
        public int AbilityIndex;
        public Entity Target;
        public Entity Owner;
        public bool Enable;
        public bool IgnorePredicates;
    }
}