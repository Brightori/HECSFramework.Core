using Components;
using HECSFramework.Core;

namespace Commands
{
    public struct FloatAnimationCommand : ICommand
    {
        public int Index;
        public float Value;
    }
}