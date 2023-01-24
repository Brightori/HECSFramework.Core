using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HECSFramework.Core;

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