using System;
using System.Collections;
using System.Collections.Generic;

namespace Graphite.Collections
{
    public class EnumerableMapper<TInner, T> : IEnumerable<T>
    {
        private readonly Lazy<EnumeratorMapper<TInner, T>> _enumerator;

        public EnumerableMapper(IEnumerator<TInner> enumerator, Func<TInner, T> map) =>
            _enumerator = new Lazy<EnumeratorMapper<TInner, T>>(() => 
                new EnumeratorMapper<TInner, T>(enumerator, map));

        public IEnumerator<T> GetEnumerator() => _enumerator.Value;
        IEnumerator IEnumerable.GetEnumerator() =>  GetEnumerator();
    }
}