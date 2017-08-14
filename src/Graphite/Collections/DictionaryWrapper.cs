using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Graphite.Collections
{
    public abstract class DictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private static readonly IDictionary<TKey, TValue> EmptyDictionary =
            new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>());

        private readonly IDictionary<TKey, TValue> _innerDictionary;

        protected DictionaryWrapper(IDictionary<TKey, TValue> innerDictionary)
        {
            _innerDictionary = innerDictionary ?? EmptyDictionary;
        }

        public virtual TValue this[TKey key]
        {
            get => _innerDictionary[key];
            set => _innerDictionary[key] = value;
        }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _innerDictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public virtual void Add(KeyValuePair<TKey, TValue> item) => _innerDictionary.Add(item);
        public virtual void Clear() => _innerDictionary.Clear();
        public virtual bool Contains(KeyValuePair<TKey, TValue> item) => _innerDictionary.Contains(item);
        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _innerDictionary.CopyTo(array, arrayIndex);
        public virtual bool Remove(KeyValuePair<TKey, TValue> item) => _innerDictionary.Remove(item);
        public virtual int Count => _innerDictionary.Count;
        public virtual bool IsReadOnly => _innerDictionary.IsReadOnly;
        public virtual bool ContainsKey(TKey key) => _innerDictionary.ContainsKey(key);
        public virtual void Add(TKey key, TValue value) => _innerDictionary.Add(key, value);
        public virtual bool Remove(TKey key) => _innerDictionary.Remove(key);
        public virtual bool TryGetValue(TKey key, out TValue value) => _innerDictionary.TryGetValue(key, out value);
        public virtual ICollection<TKey> Keys => _innerDictionary.Keys;
        public virtual ICollection<TValue> Values => _innerDictionary.Values;
    }
}