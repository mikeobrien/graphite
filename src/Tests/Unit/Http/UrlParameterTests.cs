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
    public class UrlParameterTests
    {
        public class Handler
        {
            public void Params(object request, string param1, string param2) { }
            public void WildcardParams(object request, string param1, params int[] param2) { }
            public void WildcardArray(object request, string param1, int[] param2) { }
            public void WildcardList(object request, string param1, List<int> param2) { }
            public void WildcardIList(object request, string param1, IList<int> param2) { }
            public void WildcardIEnumerable(object request, string param1, IEnumerable<int> param2) { }
            public void WildcardICollection(object request, string param1, ICollection<int> param2) { }
        }

        [Test]
        public void Should_load_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(x => x.Params(null, null, null))
                .WithUrl("http://fark.com/value1/segment/value2")
                .WithUrlTemplate("{param1}/segment/{param2}");
            
            var urlParameters = new UrlParameters(requestGraph.GetHttpRequestMessage(), 
                requestGraph.GetRouteDescriptor()).ToList();

            urlParameters.Count.ShouldEqual(2);

            var parameter = urlParameters[0];

            parameter.Key.ShouldEqual("param1");
            parameter.ShouldOnlyContain("value1");

            parameter = urlParameters[1];

            parameter.Key.ShouldEqual("param2");
            parameter.ShouldOnlyContain("value2");
        }

        public static object[][] WildcardCases = TestCaseSource
            .CreateWithExpression<Handler>(x => x
                .Add(h => h.WildcardParams(null, null))
                .Add(h => h.WildcardArray(null, null, null))
                .Add(h => h.WildcardList(null, null, null))
                .Add(h => h.WildcardIList(null, null, null))
                .Add(h => h.WildcardIEnumerable(null, null, null))
                .Add(h => h.WildcardICollection(null, null, null)));

        [TestCaseSource(nameof(WildcardCases))]
        public void Should_parse_wildcard_parameter_for_generic_list_castable_wildcard_parameter(
            Expression<Action<Handler>> action)
        {
            var requestGraph = RequestGraph
                .CreateFor(action)
                .WithUrl("http://fark.com/value1/segment/1/2/3")
                .WithUrlTemplate("{param1}/segment/{*param2}");
            
            var urlParameters = new UrlParameters(requestGraph.GetHttpRequestMessage(), 
                requestGraph.GetRouteDescriptor()).ToList();

            urlParameters.Count.ShouldEqual(2);

            var parameter = urlParameters[0];

            parameter.Key.ShouldEqual("param1");
            parameter.ShouldOnlyContain("value1");

            parameter = urlParameters[1];

            parameter.Key.ShouldEqual("param2");
            parameter.ShouldOnlyContain("1", "2", "3");
        }

        [TestCaseSource(nameof(WildcardCases))]
        public void Should_not_parse_wildcard_when_not_generic_list_castable_wildcard_parameter(
            Expression<Action<Handler>> action)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(x => x.Params(null, null, null))
                .WithUrl("http://fark.com/value1/segment/1/2/3")
                .WithUrlTemplate("{param1}/segment/{*param2}");
            
            var urlParameters = new UrlParameters(requestGraph.GetHttpRequestMessage(), 
                requestGraph.GetRouteDescriptor()).ToList();

            urlParameters.Count.ShouldEqual(2);

            var parameter = urlParameters[0];

            parameter.Key.ShouldEqual("param1");
            parameter.ShouldOnlyContain("value1");

            parameter = urlParameters[1];

            parameter.Key.ShouldEqual("param2");
            parameter.ShouldOnlyContain("1/2/3");
        }
    }
}
