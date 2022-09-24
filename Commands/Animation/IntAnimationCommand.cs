using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Animation, "animation command for int parameters")]
    public struct IntAnimationCommand : ICommand
    {
        public int Index;
        public int Value;
    }
}