using Components;
using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Animation, "this command set float parameter at" + nameof(AnimatorStateComponent))]
    public struct FloatAnimationCommand : ICommand
    {
        public int Index;
        public float Value;
    }
}