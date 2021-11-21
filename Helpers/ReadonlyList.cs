using System.Collections;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class ReadonlyList<T> : IReadOnlyList<T>
    {
        private List<T> list;

        public T this[int index] => list[index];

        public int Count => list.Count;

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        public ReadonlyList(List<T> list)
        {
            this.list = list;
        }
    }
}