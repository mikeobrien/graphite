using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Graphite.Http
{
    public class QuerystringParameters : ILookup<string, string>
    {
        private readonly ILookup<string, string> _source;

        public QuerystringParameters(ILookup<string, string> source)
        {
            _source = source;
        }

        public static QuerystringParameters CreateFrom(HttpRequestMessage message)
        {
            return new QuerystringParameters(message.GetQueryNameValuePairs()?
                .ToLookup(x => x.Key, x => x.Value));
        }

        public int Count => _source.Count;
        public IEnumerable<string> this[string key] => _source[key];

        public bool Contains(string key)
        {
            return _source.Contains(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IGrouping<string, string>> GetEnumerator()
        {
            return _source.GetEnumerator();
        }
    }
}