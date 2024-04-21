using Components;
using HECSFramework.Core;

namespace Systems
{
    public struct WaitForStateJob : IHecsJob
    {
        public World World;
        public int State;

        public WaitForStateJob(World world, int state)
        {
            World = world;
            State = state;
        }

        public bool IsComplete()
        {
            return World.GetSingleComponent<GameStateComponent>().CurrentState == State;
        }

        public void Run()
        {
        }
    }
}