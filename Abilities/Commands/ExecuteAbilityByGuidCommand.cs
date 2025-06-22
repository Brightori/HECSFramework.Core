using System;
using HECSFramework.Core;

namespace Commands
{
    public struct ExecuteAbilityByGuidCommand : ICommand
    {
        public Guid AbilityGuid;
        public Entity Target;
        public Entity Owner;
        public bool Enable;
        public bool IgnorePredicates;
    }
}