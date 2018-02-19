using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Graphite.Extensions;
using Graphite.Linq;
using Graphite.Routing;

namespace Graphite.Http
{
    public interface IQuerystringParameters : ILookup<string, object> { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class DelimitedAttribute : Attribute
    {
        public DelimitedAttribute(string delimiter = ",")
        {
            Delimiter = delimiter;
        }

        public string Delimiter { get; }
    }
    
    public class QuerystringParameters : ParametersBase<string>, IQuerystringParameters
    {
        public QuerystringParameters(HttpRequestMessage request, 
            RouteDescriptor routeDescriptor, Configuration configuration) : 
            base(GetParameters(request, routeDescriptor, configuration)) { }
        
        private static IEnumerable<KeyValuePair<string, string>> GetParameters(
            HttpRequestMessage request, RouteDescriptor routeDescriptor, Configuration configuration)
        {
            var parameters = request.GetQueryNameValuePairs();
            return (configuration.QuerystringParameterDelimiters.Any()
                    ? parameters.SelectMany(x => ParseDelimitedParameters(
                        x, routeDescriptor, configuration))
                    : parameters)
                .Where(x => x.Value.IsNotNullOrEmpty());
        }

        private static IEnumerable<KeyValuePair<string, string>> ParseDelimitedParameters
            (KeyValuePair<string, string> value, RouteDescriptor routeDescriptor, Configuration configuration)
        {
            var parameter = routeDescriptor.Parameters.FirstOrDefault(
                x => x.Name.EqualsUncase(value.Key));

            if (parameter == null || (!parameter.TypeDescriptor.IsArray &&
                !parameter.TypeDescriptor.IsGenericListCastable))
                return new[] { value };

            var delimiter = configuration.QuerystringParameterDelimiters
                .Select(x => x(parameter)).FirstOrDefault(x => x.IsNotNullOrEmpty());

            return delimiter != null && (parameter.TypeDescriptor.IsArray || 
                    parameter.TypeDescriptor.IsGenericListCastable)
                ? value.Value.Split(delimiter).ToKeyValuePairs(value.Key)
                : new[] { value };
        }
    }
}