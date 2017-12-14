using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Http
{
    [TestFixture]
    public class QuerystringParameterTests
    {
        public class Handler
        {
            public void Params(object request, string param1, [Delimited] string param2) { }
            public void ParamsParams(object request, string param1, [Delimited] params int[] param2) { }
            public void ParamsArray(object request, string param1, [Delimited] int[] param2) { }
            public void ParamsList(object request, string param1, [Delimited] List<int> param2) { }
            public void ParamsIList(object request, string param1, [Delimited] IList<int> param2) { }
            public void ParamsIEnumerable(object request, string param1, [Delimited] IEnumerable<int> param2) { }
            public void ParamsICollection(object request, string param1, [Delimited] ICollection<int> param2) { }
        }

        [Test]
        public void Should_load_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(x => x.Params(null, null, null))
                .WithUrl("http://fark.com?param1=value1&param2=value2")
                .AddParameters("param1", "param2");
            
            var querystring = new QuerystringParameters(requestGraph.GetHttpRequestMessage(), 
                requestGraph.GetRouteDescriptor(), requestGraph.Configuration).ToList();

            querystring.Count.ShouldEqual(2);

            var parameter = querystring[0];

            parameter.Key.ShouldEqual("param1");
            parameter.ShouldOnlyContain("value1");

            parameter = querystring[1];

            parameter.Key.ShouldEqual("param2");
            parameter.ShouldOnlyContain("value2");
        }

        public static object[][] WildcardCases = TestCaseSource
            .CreateWithExpression<Handler>(x => x
                .Add(h => h.ParamsParams(null, null))
                .Add(h => h.ParamsArray(null, null, null))
                .Add(h => h.ParamsList(null, null, null))
                .Add(h => h.ParamsIList(null, null, null))
                .Add(h => h.ParamsIEnumerable(null, null, null))
                .Add(h => h.ParamsICollection(null, null, null)));

        [TestCaseSource(nameof(WildcardCases))]
        public void Should_parse_params_for_generic_list_castable_parameter(
            Expression<Action<Handler>> action)
        {
            var requestGraph = RequestGraph
                .CreateFor(action)
                .WithUrl("http://fark.com/?param1=value1&param2=1,2,3")
                .AddParameters("param1", "param2");
            
            var querystring = new QuerystringParameters(requestGraph.GetHttpRequestMessage(), 
                requestGraph.GetRouteDescriptor(), requestGraph.Configuration).ToList();

            querystring.Count.ShouldEqual(2);

            var parameter = querystring[0];

            parameter.Key.ShouldEqual("param1");
            parameter.ShouldOnlyContain("value1");

            parameter = querystring[1];

            parameter.Key.ShouldEqual("param2");
            parameter.ShouldOnlyContain("1", "2", "3");
        }

        [TestCaseSource(nameof(WildcardCases))]
        public void Should_not_parse_params_when_not_generic_list_castable_parameter(
            Expression<Action<Handler>> action)
        {
            var requestGraph = RequestGraph
                .CreateFor<UrlParameterTests.Handler>(x => x.Params(null, null, null))
                .WithUrl("http://fark.com/?param1=value1&param2=1,2,3")
                .AddParameters("param1", "param2");
            
            var querystring = new QuerystringParameters(requestGraph.GetHttpRequestMessage(), 
                requestGraph.GetRouteDescriptor(), requestGraph.Configuration).ToList();

            querystring.Count.ShouldEqual(2);

            var parameter = querystring[0];

            parameter.Key.ShouldEqual("param1");
            parameter.ShouldOnlyContain("value1");

            parameter = querystring[1];

            parameter.Key.ShouldEqual("param2");
            parameter.ShouldOnlyContain("1,2,3");
        }
    }
}
