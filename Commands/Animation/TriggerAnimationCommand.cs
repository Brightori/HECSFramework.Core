using Components;
using HECSFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands
{
    public struct TriggerAnimationCommand : ICommand
    {
        public int Index;
    }
}
