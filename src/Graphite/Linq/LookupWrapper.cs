using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Graphite.Linq
{
    public abstract class LookupWrapper<TKey, TValue> : ILookup<TKey, TValue>
    {
        private static readonly ILookup<TKey, TValue> EmptyLookup = 
            new Dictionary<TKey, TValue>().ToLookup(x => x.Key, x => x.Value);

        private readonly ILookup<TKey, TValue> _source;

        protected LookupWrapper(IEnumerable<KeyValuePair<TKey, TValue>> source,
            IEqualityComparer<TKey> equalityComparer)
        {
            _source = source?.ToLookup(x => x.Key, x => x.Value, equalityComparer) ?? EmptyLookup;
        }

        public virtual int Count => _source.Count;
        public virtual IEnumerable<TValue> this[TKey key] => _source[key];

        public virtual bool Contains(TKey key)
        {
            return _source.Contains(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual IEnumerator<IGrouping<TKey, TValue>> GetEnumerator()
        {
            return _source.GetEnumerator();
        }
    }
}