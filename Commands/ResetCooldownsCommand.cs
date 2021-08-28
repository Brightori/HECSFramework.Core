using System;
using HECSFramework.Core;

namespace Commands
{
    public struct ResetCooldownsCommand : IGlobalCommand
    {
        public Guid Target;
    }
}