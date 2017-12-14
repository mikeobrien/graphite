using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Graphite.Linq
{
    public class ValueGrouping<TKey, TValue> : IGrouping<TKey, TValue>
    {
        private readonly TValue _value;

        public ValueGrouping(TKey key, TValue value)
        {
            _value = value;
            Key = key;
        }

        public TKey Key { get; }

        public IEnumerator<TValue> GetEnumerator()
        {
            yield return _value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}