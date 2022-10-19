using Components;
using HECSFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands
{
    [Documentation(Doc.Animation, "this command set bool parameter at" + nameof(AnimatorStateComponent))]
    public struct TriggerAnimationCommand : ICommand
    {
        public int Index;
    }
}
