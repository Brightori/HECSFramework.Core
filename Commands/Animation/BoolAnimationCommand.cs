using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Animation, "this command set bool parameter at AnimatorStateComponent")]
    public struct BoolAnimationCommand : ICommand
    {
        public int Index;
        public bool Value;
        public bool ForceSet;
    }
}