using System;
using System.Linq;
using Graphite.Routing;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        public class QuerystringDelimiterDsl
        {
            private readonly Configuration _configuration;

            public QuerystringDelimiterDsl(Configuration configuration)
            {
                _configuration = configuration;
            }

            /// <summary>
            /// Clears all delimiter conventions.
            /// </summary>
            public QuerystringDelimiterDsl Clear()
            {
                _configuration.QuerystringParameterDelimiters.Clear();
                return this;
            }

            /// <summary>
            /// Appends a delimiter convention.
            /// </summary>
            public QuerystringDelimiterDsl Append(Func<ActionParameter, string> convention)
            {
                _configuration.QuerystringParameterDelimiters.Add(convention);
                return this;
            }

            /// <summary>
            /// Prepends a delimiter convention.
            /// </summary>
            public QuerystringDelimiterDsl Prepend(Func<ActionParameter, string> convention)
            {
                if (_configuration.QuerystringParameterDelimiters.Any())
                    _configuration.QuerystringParameterDelimiters.Insert(0, convention);
                else Append(convention);
                return this;
            }
        }

        /// <summary>
        /// Configure querystring parameter delimiter conventions.
        /// </summary>
        public ConfigurationDsl ConfigureQuerystringParameterDelimiters(
            Action<QuerystringDelimiterDsl> config)
        {
            config(new QuerystringDelimiterDsl(_configuration));
            return this;
        }
    }
}
