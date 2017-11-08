using System;
using System.Collections;
using System.Collections.Generic;

namespace Graphite.Collections
{
    public class EnumeratorMapper<TInner, T> : IEnumerator<T>
    {
        private readonly IEnumerator<TInner> _enumerator;
        private readonly Func<TInner, T> _map;

        public EnumeratorMapper(IEnumerator<TInner> enumerator, Func<TInner, T> map)
        {
            _enumerator = enumerator;
            _map = map;
        }

        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => _enumerator.Reset();
        public T Current => _map(_enumerator.Current);
        object IEnumerator.Current => Current;
        public void Dispose() => _enumerator.Dispose();
    }
}