using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Graphite.Http
{
    public abstract class ParametersBase<TValue> : ILookup<string, object>
        where TValue : class
    {
        private static readonly IEnumerable<object> Empty = new object [] { };
        private readonly Dictionary<string, IGrouping<string, TValue>> _parameters;

        protected ParametersBase(IEnumerable<KeyValuePair<string, TValue>> parameters)
        {
            _parameters = parameters
                .GroupBy(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase);
        }

        public int Count => _parameters.Count;

        public bool Contains(string key) => _parameters.ContainsKey(key);

        public IEnumerable<object> this[string key] => 
            Contains(key)
                ? _parameters[key]
                : Empty;

        public IEnumerator<IGrouping<string, object>> GetEnumerator()
        {
            return _parameters.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}