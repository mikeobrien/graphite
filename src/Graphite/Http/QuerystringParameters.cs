using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Graphite.Linq;

namespace Graphite.Http
{
    public class QuerystringParameters : LookupWrapper<string, object>
    {
        public QuerystringParameters() : base(null, null) { }

        public QuerystringParameters(IEnumerable<KeyValuePair<string, string>> source):
            base(source?.Select(x => new KeyValuePair<string, object>(x.Key, x.Value)), 
                StringComparer.OrdinalIgnoreCase) { }

        public static QuerystringParameters CreateFrom(HttpRequestMessage message)
        {
            return new QuerystringParameters(message.GetQueryNameValuePairs());
        }
    }
}