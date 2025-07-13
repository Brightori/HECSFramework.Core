using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Animation, "this command play animation by hash of state, u should definitly now name of state")]
    public struct PlayAnimationCommand : ICommand
    {
        public int Index;
        public int Layer;
        public float NormalizedTime;
    }
}