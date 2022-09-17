using HECSFramework.Core;

namespace Systems
{
    [Documentation(Doc.Server, Doc.GameLogic, "This system for update awaiters, she should be only on server world, and init from main server thread")]
    public class JobUpdateSystem : BaseSystem, IUpdatable, IOnThreadStartInit
    {
        private Job job;

        public override void InitSystem()
        {
            
        }

        public void OnThreadStartInit()
        {
            job = JobsSystem.RegisterThreadHandler();
        }

        public void UpdateLocal()
        {
            job.Update();
        }
    }
}