using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class UpdateModuleGlobalStart : IGlobalStart, IRegisterUpdate<IGlobalStart>, IRegisterUpdate<ILateStart>
    {
        private readonly Queue<IGlobalStart> globalStartups = new Queue<IGlobalStart>(64);
        private readonly Queue<ILateStart> lateStartups = new Queue<ILateStart>(64);
        public bool IsStarted { get; private set; }
        public bool IsLateStarted { get; private set; } 

        public void Dispose()
        {
            globalStartups.Clear();
            lateStartups.Clear();
        }

        public void GlobalStart()
        {
            IsStarted = true;
            
            while (globalStartups.Count > 0)
                globalStartups.Dequeue().GlobalStart();
        }

        public void LateStart()
        {
            IsLateStarted = true;

            while (lateStartups.Count > 0)
                lateStartups.Dequeue().LateStart();
        }

        //тут адд не нужен, но мы оставляем его ради однообразия сигнатуры
        //we dont needed bool add here, but we keep it for same signature in all register methods 
        public void Register(IGlobalStart updatable, bool add)
        {
            if (!add)
                return;

            if (IsStarted)
                updatable.GlobalStart();
            else
            {
                if (add)
                    globalStartups.Enqueue(updatable);
            }
        }

        public void Register(ILateStart updatable, bool add)
        {
            if (IsLateStarted && add)
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

    /// <summary>
    /// we use it for server worlds and we need start with server but on server thread
    /// </summary>
    public interface IOnThreadStartInit 
    { 
        void OnThreadStartInit();
    }

    public interface ILateStart : IRegisterUpdatable
    {
        void LateStart();
    }
}