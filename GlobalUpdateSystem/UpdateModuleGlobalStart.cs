using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class UpdateModuleGlobalStart : INeedGlobalStart, IRegisterUpdate<INeedGlobalStart>
    {
        private readonly Queue<INeedGlobalStart> globalStartups = new Queue<INeedGlobalStart>(64);
        private bool isStarted;
       
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void GlobalStart()
        {
            isStarted = true;
            
            while (globalStartups.Count > 0)
                globalStartups.Dequeue().GlobalStart();
        }

        //тут адд не нужен, но мы оставляем его ради однообразия сигнатуры
        //we dont needed bool add here, but we keep it for same signature in all register methods 
        public void Register(INeedGlobalStart updatable, bool add)
        {
            if (isStarted)
                updatable.GlobalStart();
            else
            {
                if (add)
                    globalStartups.Enqueue(updatable);
            }
        }
    }
    
    public interface INeedGlobalStart : IRegisterUpdatable
    {
        void GlobalStart();
    }
}