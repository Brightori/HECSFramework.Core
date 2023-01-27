using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Animation, "this command set bool parameter at AnimatorStateComponent")]
    public struct TriggerAnimationCommand : ICommand
    {
        public int Index;
    }
}
