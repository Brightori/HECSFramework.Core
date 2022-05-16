using System.Collections.Generic;

namespace HECSFramework.Core
{
    [Documentation(Doc.HECS, Doc.Helpers, "This is helper object for safely removing elements from concurrency list, u add on iteration elements to remove, and after iteration call method - process removing")]
    public class Remover<T>
    {
        private ConcurrencyList<T> concurrencyList;
        private Queue<T> queue = new Queue<T>(8);
        private bool isDirty;

        public Remover(ConcurrencyList<T> concurrencyList)
        {
            this.concurrencyList = concurrencyList;
        }

        public void Add(T element)
        {
            queue.Enqueue(element);
            isDirty = true;
        }

        public void ProcessRemoving()
        {
            if (isDirty)
            {
                while (queue.Count > 0)
                    concurrencyList.Remove(queue.Dequeue());

                isDirty = false;
            }
        }
    }
}
