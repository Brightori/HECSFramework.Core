using System.Collections;
using System.Collections.Generic;

namespace HECSFramework.Core
{
    public class ReadOnlyDictionary<Tkey, Tvalue> : IReadOnlyDictionary<Tkey, Tvalue>
    {
        private Dictionary<Tkey, Tvalue> dict;

        public ReadOnlyDictionary(Dictionary<Tkey, Tvalue> dict)
        {
            this.dict = dict;
        }

        public Tvalue this[Tkey key] => dict[key];

        public IEnumerable<Tkey> Keys => dict.Keys;
        public IEnumerable<Tvalue> Values => dict.Values;
        public int Count => dict.Count;

        public bool ContainsKey(Tkey key)
        {
            return dict.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<Tkey, Tvalue>> GetEnumerator() => dict.GetEnumerator();

        public bool TryGetValue(Tkey key, out Tvalue value)
        {
            return dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();
    }
}