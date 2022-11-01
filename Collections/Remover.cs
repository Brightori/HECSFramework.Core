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

    [Documentation(Doc.HECS, Doc.Helpers, "This is helper object for safely removing or adding elements from concurrency list, u add on iteration elements to remove, and after iteration call method - " + nameof(Process))]
    public class AddAndRemoveHelper<T>
    {
        private ConcurrencyList<T> concurrencyList;
        private Queue<T> add = new Queue<T>(8);
        private Queue<T> remove = new Queue<T>(8);
        private bool isDirty;

        public AddAndRemoveHelper(ConcurrencyList<T> concurrencyList)
        {
            this.concurrencyList = concurrencyList;
        }

        public void Add(T element)
        {
            add.Enqueue(element);
            isDirty = true;
        }

        public void Remove(T element)
        {
            remove.Enqueue(element);
            isDirty=true;
        }

        public void Process()
        {
            if (isDirty)
            {
                while (remove.Count > 0)
                    concurrencyList.Remove(remove.Dequeue());

                while(add.Count>0)
                    concurrencyList.Add(add.Dequeue());

                isDirty = false;
            }
        }
    }
}
