using System;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class DispatchModule : IUpdatable
    {
        private List<Func<bool>> dispatchItems = new List<Func<bool>>(4);
        private Queue<Func<bool>> remove = new Queue<Func<bool>>(4);

        public void UpdateLocal()
        {
            var count = dispatchItems.Count;

            for (int i = 0; i < count; i++)
            {
                var func = dispatchItems[i];

                if (func())
                    remove.Enqueue(func);
            }

            while(remove.Count > 0)
                dispatchItems.Remove(remove.Dequeue());
        }

        public void AddToDispatch(Func<bool> func)
        {
            dispatchItems.Add(func);
        }
    }
}
