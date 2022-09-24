using Components;
using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Animation, "this command set bool parameter at" + nameof(AnimatorStateComponent))]
    public struct BoolAnimationCommand : ICommand
    {
        public int Index;
        public bool Value;
    }
}