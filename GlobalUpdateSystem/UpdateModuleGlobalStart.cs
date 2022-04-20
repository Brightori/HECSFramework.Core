using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class UpdateModuleGlobalStart : IGlobalStart, IRegisterUpdate<IGlobalStart>, IRegisterUpdate<ILateStart>
    {
        private readonly Queue<IGlobalStart> globalStartups = new Queue<IGlobalStart>(64);
        private readonly Queue<ILateStart> lateStartups = new Queue<ILateStart>(64);
        private bool isStarted;
        private bool isLateStarted;

        public void Dispose()
        {
            globalStartups.Clear();
            lateStartups.Clear();
        }

        public void GlobalStart()
        {
            isStarted = true;
            
            while (globalStartups.Count > 0)
                globalStartups.Dequeue().GlobalStart();
        }

        public void LateStart()
        {
            isLateStarted = true;

            while (lateStartups.Count > 0)
                lateStartups.Dequeue().LateStart();
        }

        //тут адд не нужен, но мы оставляем его ради однообразия сигнатуры
        //we dont needed bool add here, but we keep it for same signature in all register methods 
        public void Register(IGlobalStart updatable, bool add)
        {
            if (isStarted)
                updatable.GlobalStart();
            else
            {
                if (add)
                    globalStartups.Enqueue(updatable);
            }
        }

        public void Register(ILateStart updatable, bool add)
        {
            if (isLateStarted && add)
                updatable.LateStart();
            else
            {
                if (add)
                    lateStartups.Enqueue(updatable);
            }
        }
    }
    
    public interface IGlobalStart : IRegisterUpdatable
    {
        void GlobalStart();
    }

    public interface ILateStart : IRegisterUpdatable
    {
        void LateStart();
    }
}