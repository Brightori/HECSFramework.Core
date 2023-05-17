using HECSFramework.Core;

namespace Systems
{
    [Documentation(Doc.Server, Doc.GameLogic, "This system for update awaiters, she should be only on server world, and init from main server thread")]
    public class AwaitersUpdateSystem : BaseSystem, IUpdatable, IOnThreadStartInit
    {
        private AwaiterProcessor awaiterProcessor;

        private bool isOnline;

        public override void InitSystem()
        {
            
        }

        public void OnThreadStartInit()
        {
            awaiterProcessor = AwaitersService.RegisterThreadHandler();
            isOnline = true;
        }

        public void UpdateLocal()
        {
            if (isOnline)
                awaiterProcessor.Update();
        }
    }
}