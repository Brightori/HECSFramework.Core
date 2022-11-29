using Components;
using HECSFramework.Core;

namespace Commands
{
    public struct BoolAnimationCommand : ICommand
    {
        public int Index;
        public bool Value;
    }
}