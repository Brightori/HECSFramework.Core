using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class Remover<T>
    {
        private ConcurrencyList<T> concurrencyList;
        private Queue<T> queue = new Queue<T>(8);

        public Remover(ConcurrencyList<T> concurrencyList)
        {
            this.concurrencyList = concurrencyList;
        }

        public void Add(T element)
        {
            queue.Enqueue(element);
        }

        public void ProcessRemoving()
        {
            while (queue.Count > 0)
                concurrencyList.Remove(queue.Dequeue());
        }
    }
}
