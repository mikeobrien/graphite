using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Routing;
using Tests.Common;
using NUnit.Framework;
using Should;
using Tests.Common.Fakes;
using Range = Graphite.Routing.RangeAttribute;

namespace Tests.Unit.Routing
{
    [TestFixture]
    public class DefaultRouteConventionTests
    {
        private Configuration _configuration;
        private List<IUrlConvention> _urlConventions;
        private DefaultRouteConvention _routeConvention;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _urlConventions = new List<IUrlConvention>
            {
                new DefaultUrlConvention(_configuration, null)
            };
            _configuration.UrlConventions.Configure(c => c.Append<DefaultUrlConvention>());
            _routeConvention = new DefaultRouteConvention(
                _configuration, null, _urlConventions,
                new DefaultInlineConstraintBuilder(_configuration));
        }

        public class Request { }
        public class Response { }

        public class Handler
        {
            public Response Post_UrlParam1_Segment_UrlParam2(Request request,
                string urlParam1, Guid? urlParam2, string param1, Guid? param2)
            {
                return null;
            }
        }

        [Test]
        public void Should_get_route_descriptor_for_action()
        {
            var descriptors = _routeConvention.GetRouteDescriptors(new RouteContext(
                ActionMethod.From<Handler>(x => x.Post_UrlParam1_Segment_UrlParam2(
                    null, null, null, null, null))));

            descriptors.Count.ShouldEqual(1);

            var descriptor = descriptors.First();
            var url = "Unit/Routing/{urlParam1}/Segment/{urlParam2}";

            descriptor.Id.ShouldEqual($"POST:{url.RemoveParameterNames()}");
            descriptor.Method.ShouldEqual("POST");
            descriptor.Url.ShouldEqual(url);

            descriptor.UrlParameters.Count().ShouldEqual(2);
            descriptor.UrlParameters.ShouldContain(x => x.Name == "urlParam1");
            descriptor.UrlParameters.ShouldContain(x => x.Name == "urlParam2");

            descriptor.Parameters.Count().ShouldEqual(2);
            descriptor.Parameters.ShouldContain(x => x.Name == "param1");
            descriptor.Parameters.ShouldContain(x => x.Name == "param2");

            descriptor.HasRequest.ShouldBeTrue();
            descriptor.RequestParameter.ParameterType.Type.ShouldEqual<Request>();

            descriptor.HasResponse.ShouldBeTrue();
            descriptor.ResponseType.Type.ShouldEqual<Response>();
        }

        public class VerbHandler
        {
            public void Get() { }
            public void Post() { }
            public void Put() { }
            public void Delete() { }
            public void Options() { }
            public void Head() { }
            public void Trace() { }
            public void Connect() { }

            public void Get_Segment() { }
            public void Post_Segment() { }
            public void Put_Segment() { }
            public void Delete_Segment() { }
            public void Options_Segment() { }
            public void Head_Segment() { }
            public void Trace_Segment() { }
            public void Connect_Segment() { }

            public void NotASupportedVerb() { }
        }

        public static object[][] HttpMethodCases = TestCaseSource
            .CreateWithExpression<VerbHandler>(x => x
                .Add(h => h.Get()).Add(h => h.Post()).Add(h => h.Put())
                .Add(h => h.Delete()).Add(h => h.Options()).Add(h => h.Head())
                .Add(h => h.Trace()).Add(h => h.Connect())

                .Add(h => h.Get_Segment()).Add(h => h.Post_Segment()).Add(h => h.Put_Segment())
                .Add(h => h.Delete_Segment()).Add(h => h.Options_Segment()).Add(h => h.Head_Segment())
                .Add(h => h.Trace_Segment()).Add(h => h.Connect_Segment()));

        [TestCaseSource(nameof(HttpMethodCases))]
        public void Should_get_all_common_http_verbs(Expression<Action<VerbHandler>> action)
        {
            var descriptors = _routeConvention.GetRouteDescriptors(
                new RouteContext(ActionMethod.From(action)));

            descriptors.Count.ShouldEqual(1);
            descriptors.First().Method.ShouldEqual(action.GetMethodInfo()
                .Name.Replace("_Segment", "").ToUpper());
        }

        [Test]
        public void Should_fail_to_get_unsupported_verb()
        {
            _routeConvention.Should().Throw<InvalidOperationException>(
                x => x.GetRouteDescriptors(new RouteContext(ActionMethod
                    .From<VerbHandler>(h => h.NotASupportedVerb()))))
                .Message.ShouldContainAll(typeof(VerbHandler).FullName, 
                    nameof(VerbHandler.NotASupportedVerb));
        }

        public struct ValueTypeParam { }

        public class RequestHandler
        {
            public void Put(Request request) { }
            public void Post(Request request) { }
            public void Delete(Request request) { }

            public void Get(Request request) { }
            public void Options(Request request) { }
            public void Head(Request request) { }
            public void Trace(Request request) { }
            public void Connect(Request request) { }

            public void Get_NoRequest() { }
            public void Get_UrlParam(string urlParam) { }
            public void Get_ValueTypeUrlParam(ValueTypeParam urlParam) { }
        }

        public static object[][] RequestBodySupportMethodCases = TestCaseSource
            .CreateWithExpression<RequestHandler>(x => x
                .Add(h => h.Put(null))
                .Add(h => h.Post(null))
                .Add(h => h.Delete(null)));

        [TestCaseSource(nameof(RequestBodySupportMethodCases))]
        public void Should_get_request_parameter_for_methods_that_support_a_request_body(
            Expression<Action<RequestHandler>> action)
        {
            var descriptors = _routeConvention.GetRouteDescriptors(
                new RouteContext(ActionMethod.From(action)));

            descriptors.Count.ShouldEqual(1);
            var descriptor = descriptors.First();
            descriptor.HasRequest.ShouldBeTrue();
            descriptor.RequestParameter.ParameterType.Type.ShouldEqual<Request>();
        }

        public static object[][] RequestBodyNotSupportedMethodCases = TestCaseSource
            .CreateWithExpression<RequestHandler>(x => x
                .Add(h => h.Get(null))
                .Add(h => h.Options(null))
                .Add(h => h.Head(null)));

        [TestCaseSource(nameof(RequestBodyNotSupportedMethodCases))]
        public void Should_not_get_request_parameter_for_methods_that_do_not_support_a_request_body(
            Expression<Action<RequestHandler>> action)
        {
            var descriptors = _routeConvention.GetRouteDescriptors(
                new RouteContext(ActionMethod.From(action)));

            descriptors.Count.ShouldEqual(1);
            var descriptor = descriptors.First();
            descriptor.HasRequest.ShouldBeFalse();
            descriptor.RequestParameter.ShouldBeNull();
        }

        public static object[][] NoRequestCases = TestCaseSource
            .CreateWithExpression<RequestHandler>(x => x
                .Add(h => h.Get_NoRequest())
                .Add(h => h.Get_UrlParam(null))
                .Add(h => h.Get_ValueTypeUrlParam(new ValueTypeParam())));

        [TestCaseSource(nameof(NoRequestCases))]
        public void Should_not_get_request_parameter_if_not_specified(
            Expression<Action<RequestHandler>> action)
        {
            var descriptors = _routeConvention.GetRouteDescriptors(
                new RouteContext(ActionMethod.From(action)));

            descriptors.Count.ShouldEqual(1);
            var descriptor = descriptors.First();
            descriptor.HasRequest.ShouldBeFalse();
            descriptor.RequestParameter.ShouldBeNull();
        }

        public class ParametersModel
        {
            public string Param { get; set; }
            public string Param2 { get; }
            public ParametersModel Model { get; set; }
        }

        public class ParametersHandler
        {
            public void Get() { }
            public void Get_() { }
            public void Get(string param) { }
            public void Get(ParametersModel model) { }
            public void Get_(string param) { }
            public void Get_Param(string param) { }
            public void Get_Param(ParametersModel model) { }
            public void Post(string param) { }
            public void Post_UrlParam(string urlParam) { }
            public void Post_UrlParam(string[] urlParam) { }
            public void Post_UrlParam_FromBody([FromBody] string urlParam) { }
            public void Post_UrlParam_FromBody([FromBody] string[] urlParam) { }
            public void Post_UrlParam(Request urlParam) { }
            public void Post_UrlParam(Request[] urlParam) { }
            public void Post_UrlParam_FromUri([FromUri] Request urlParam) { }
            public void Post_UrlParam_FromUri([FromUri] Request[] urlParam) { }
            public void Post_Param(string param) { }
            public void Post_Param(string[] param) { }
            public void Post_Param_FromBody([FromBody] string param) { }
            public void Post_Param_FromBody([FromBody] string[] param) { }
            public void Post_Param(Request param) { }
            public void Post_Param(Request[] param) { }
            public void Post_Param_FromUri([FromUri] Request param) { }
            public void Post_Param_FromUri([FromUri] Request[] param) { }
            public void Post(Request param) { }
            public void Post(string param1, Request param2) { }
            public void Get_UrlParam(string urlParam, string param) { }
            public void Get_Segment_Param_Segment(string param) { }
            public void Get_Segment_UrlParam_Segment(string urlParam, string param) { }
            public void Get_param(string param) { }
            public void Get__UrlParam_(string urlParam, string param) { }
            public void Get_UrlParam1_UrlParam2(string urlParam1, string urlParam2, 
                string param1, string param2) { }
            public void Get_Segment_UrlParam1_Segment_UrlParam2_Segment(string urlParam1, string urlParam2,
                string param1, string param2) { }
        }

        public static object[][] ParameterCases = TestCaseSource
            .CreateWithExpression<ParametersHandler, string, string, string, bool>(x => x
                .Add(h => h.Get(), null, null, null, false)
                .Add(h => h.Get_(), null, null, null, false)
                .Add(h => h.Get((string)null), null, null, "param", false)
                .Add(h => h.Get((ParametersModel)null), null, null, "model", false)
                .Add(h => h.Get((ParametersModel)null), null, null, "model,param", true)
                .Add(h => h.Get_(null), null, null, "param", false)
                .Add(h => h.Get_Param((string)null), null, "param", null, false)
                .Add(h => h.Get_Param((ParametersModel)null), null, null, "model", false)
                .Add(h => h.Get_Param((ParametersModel)null), null, "param", "model", true)
                .Add(h => h.Post((string)null), null, null, "param", false)
                .Add(h => h.Post_UrlParam((string)null), null, "urlParam", null, false)
                .Add(h => h.Post_UrlParam((string[])null), null, "urlParam", null, false)
                .Add(h => h.Post_UrlParam_FromBody((string)null), "urlParam", null, null, false)
                .Add(h => h.Post_UrlParam_FromBody((string[])null), "urlParam", null, null, false)
                .Add(h => h.Post_UrlParam((Request)null), "urlParam", null, null, false)
                .Add(h => h.Post_UrlParam((Request[])null), "urlParam", null, null, false)
                .Add(h => h.Post_UrlParam_FromUri((Request)null), null, "urlParam", null, false)
                .Add(h => h.Post_UrlParam_FromUri((Request[])null), null, "urlParam", null, false)
                .Add(h => h.Post_Param((string)null), null, "param", null, false)
                .Add(h => h.Post_Param((string[])null), null, "param", null, false)
                .Add(h => h.Post_Param_FromBody((string)null), "param", null, null, false)
                .Add(h => h.Post_Param_FromBody((string[])null), "param", null, null, false)
                .Add(h => h.Post_Param((Request)null), "param", null, null, false)
                .Add(h => h.Post_Param((Request[])null), "param", null, null, false)
                .Add(h => h.Post_Param_FromUri((Request)null), null, "param", null, false)
                .Add(h => h.Post_Param_FromUri((Request[])null), null, "param", null, false)
                .Add(h => h.Post((Request)null), "param", null, null, false)
                .Add(h => h.Post(null, null), "param2", null, "param1", false)
                .Add(h => h.Get_UrlParam(null, null), null, "urlParam", "param", false)
                .Add(h => h.Get_Segment_Param_Segment(null), null, "param", null, false)
                .Add(h => h.Get_Segment_UrlParam_Segment(null, null), null, "urlParam", "param", false)
                .Add(h => h.Get_param(null), null, "param", null, false)
                .Add(h => h.Get__UrlParam_(null, null), null, "urlParam", "param", false)
                .Add(h => h.Get_UrlParam1_UrlParam2(null, null, null, null), null, "urlParam1,urlParam2", 
                    "param1,param2", false)
                .Add(h => h.Get_Segment_UrlParam1_Segment_UrlParam2_Segment(null, null, null, null),
                    null, "urlParam1,urlParam2", "param1,param2", false));

        [TestCaseSource(nameof(ParameterCases))]
        public void Should_get_parameters(Expression<Action<ParametersHandler>> action, 
            string requestParam, string urlParams, string @params, bool bindComplexProperties)
        {
            _configuration.BindComplexTypeProperties = bindComplexProperties;
            var descriptors = _routeConvention.GetRouteDescriptors(
                new RouteContext(ActionMethod.From(action)));
            var urlParameterNames = urlParams?.Split(',') ?? new string[] {};
            var parameterNames = @params?.Split(',') ?? new string[] { };

            descriptors.Count.ShouldEqual(1);
            var descriptor = descriptors.First();

            if (requestParam == null) descriptor.RequestParameter.ShouldBeNull();
            else descriptor.RequestParameter.Name.ShouldEqual(requestParam);

            var urlParameters = descriptor.UrlParameters;
            urlParameters.Count().ShouldEqual(urlParameterNames.Length);

            foreach (var name in urlParameterNames)
            {
                urlParameters.ShouldContain(x => x.Name.EqualsUncase(name));
            }

            var parameters = descriptor.Parameters;
            parameters.Count().ShouldEqual(parameterNames.Length);

            foreach (var name in parameterNames)
            {
                parameters.ShouldContain(x => x.Name.EqualsUncase(name));
            }
        }

        public class WildcardHandler
        {
            public void GetNoWildcard_Values(string[] values) { }
            public void GetParams_Value1_Values(string value1, params string[] values) { }
            public void GetWildcardAttributeArray_Value1_Values(
                string value1, [Wildcard] string[] values) { }
            public void GetWildcardAttributeSingleValue_Value1_Values(
                string value1, [Wildcard] string values) { }
            public void GetWildcardAttributeNotLast_Value1_Values(
                [Wildcard] string[] values, string value1) { }
            public void GetWildcardAttributeMultiple_Values1_Values2(
                [Wildcard] string[] values1, [Wildcard] string[] values2) { }
        }

        public static object[][] WildcardParameterCases = TestCaseSource
           .CreateWithExpression<WildcardHandler, bool>(x => x
               .Add(h => h.GetNoWildcard_Values(null), false)
               .Add(h => h.GetParams_Value1_Values(null), true)
               .Add(h => h.GetWildcardAttributeArray_Value1_Values(null, null), true)
               .Add(h => h.GetWildcardAttributeSingleValue_Value1_Values(null, null), true)
               .Add(h => h.GetWildcardAttributeNotLast_Value1_Values(null, null), true));

        [TestCaseSource(nameof(WildcardParameterCases))]
        public void Should_make_param_wild_card(Expression<Action<WildcardHandler>> action, bool wildcard)
        {
            var routeDescriptor =_routeConvention.GetRouteDescriptors(
                new RouteContext(ActionMethod.From(action))).FirstOrDefault();

            routeDescriptor.Url.Split('/').ShouldContain($"{{{(wildcard ? "*" : "")}values}}");
            routeDescriptor.UrlParameters.Any(x => x.IsWildcard && x.Name == "values").ShouldEqual(wildcard);
        }

        [Test]
        public void Should_fail_if_multiple_wildcards_specified()
        {
            var action = ActionMethod.From<WildcardHandler>(t => t
                .GetWildcardAttributeMultiple_Values1_Values2(null, null));

            var message = _routeConvention.Should().Throw<InvalidOperationException>(x => x
                .GetRouteDescriptors(
                    new RouteContext(action))).Message;

            message.ShouldContain(action.MethodDescriptor.FriendlyName);
        }

        public class RouteConstraintHandler
        {
            public void Get_Segment_Value([Range(5, 10)] int value) { }
        }

        [Test]
        public void Should_add_route_constraints()
        {
            var routeDescriptor = _routeConvention.GetRouteDescriptors(new RouteContext(
                ActionMethod.From<RouteConstraintHandler>(x => x.Get_Segment_Value(0)))).FirstOrDefault();

            routeDescriptor.Url.ShouldEqual("Unit/Routing/Segment/{value:range(5,10)}");
        }

        public class ResponseHandler
        {
            public void Get_NoResponse() { }
            public Task Get_AsyncNoResponse() { return null; }
            public Response Get_Response() { return null; }
            public Task<Response> Get_AsyncResponse() { return null; }
        }

        public static object[][] ResponseCases = TestCaseSource
            .CreateWithExpression<ResponseHandler, bool>(x => x
                .Add(h => h.Get_NoResponse(), false)
                .Add(h => h.Get_AsyncNoResponse(), false)
                .Add(h => h.Get_Response(), true)
                .Add(h => h.Get_AsyncResponse(), true));

        [TestCaseSource(nameof(ResponseCases))]
        public void Should_get_response_if_exists(Expression<Action<ResponseHandler>> action, bool hasResponse)
        {
            var descriptors = _routeConvention.GetRouteDescriptors(
                new RouteContext(ActionMethod.From(action)));

            descriptors.Count.ShouldEqual(1);
            var descriptor = descriptors.First();

            descriptor.HasResponse.ShouldEqual(hasResponse);

            if (hasResponse) descriptor.ResponseType.Type.ShouldEqual<Response>();
            else descriptor.ResponseType.ShouldBeNull();
        }

        public class UrlHandler
        {
            public void Get() { }
            public void Get_Segment() { }
            public void Get_Segment1_Segment2() { }
            public void Get_Segment1_UrlParameter_Segment2(
                string urlParameter, string parameter) { }
            public void Get_Segment1_UrlParameter1_Segment2_UrlParameter2(
                string urlParameter1, string urlParameter2) { }
        }

        public static object[][] UrlCases = TestCaseSource
            .CreateWithExpression<UrlHandler, string>(x => x
                .Add(h => h.Get(), "")
                .Add(h => h.Get_Segment(), "Segment")
                .Add(h => h.Get_Segment1_Segment2(), "Segment1/Segment2")
                .Add(h => h.Get_Segment1_UrlParameter_Segment2(null, null), 
                    "Segment1/{urlParameter}/Segment2")
                .Add(h => h.Get_Segment1_UrlParameter1_Segment2_UrlParameter2(null, null),
                    "Segment1/{urlParameter1}/Segment2/{urlParameter2}"));

        [TestCaseSource(nameof(UrlCases))]
        public void Should_build_url(Expression<Action<UrlHandler>> action, string url)
        {
            var descriptors = _routeConvention.GetRouteDescriptors(
                new RouteContext(ActionMethod.From(action)));

            descriptors.Count.ShouldEqual(1);
            var descriptor = descriptors.First();

            descriptor.Url.ShouldEqual($"Unit/Routing/{url}".Trim('/'));
        }

        public class UrlConventionHandler
        {
            public void Get_Segment() { }
        }

        [Test]
        public void Should_use_url_conventions_that_apply()
        {
            var actionMethod = ActionMethod.From<UrlConventionHandler>(x => x.Get_Segment());
            var urlConvention = new TestUrlConvention
            {
                AppliesToFunc = c => true,
                GetUrlsFunc = c => new[] {"fark", "farker"}.Select(y => 
                    $"{y}/{c.ActionMethod.MethodDescriptor.Name}/" + 
                    $"{c.ActionMethod.HandlerTypeDescriptor.Type.Namespace.Replace(".", "/")}/" +
                    $"{c.MethodSegments.ToUrl()}").ToArray()
            };
            _urlConventions.Add(urlConvention);
            _configuration.UrlConventions.Configure(x => x.Append(urlConvention));
                
            var descriptors = _routeConvention.GetRouteDescriptors(
                new RouteContext(actionMethod));

            descriptors.Count.ShouldEqual(3);

            var defaultUrl = "Unit/Routing/Segment";

            descriptors[0].Url.ShouldEqual(defaultUrl);
            descriptors[1].Url.ShouldEqual("fark/Get_Segment/Tests/Unit/Routing/Segment");
            descriptors[2].Url.ShouldEqual("farker/Get_Segment/Tests/Unit/Routing/Segment");

            urlConvention.AppliesToCalled.ShouldBeTrue();
            urlConvention.AppliesToContext.ActionMethod.ShouldEqual(actionMethod);
            urlConvention.AppliesToContext.HttpMethod.ShouldEqual("GET");
            urlConvention.AppliesToContext.MethodSegments
                .ToUrl().ShouldEqual("Segment");
            urlConvention.AppliesToContext.UrlParameters.ShouldBeEmpty();
            urlConvention.AppliesToContext.Parameters.ShouldBeEmpty();
            urlConvention.AppliesToContext.RequestParameter.ShouldEqual(null);
            urlConvention.AppliesToContext.ResponseType.ShouldEqual(null);

            urlConvention.GetUrlsCalled.ShouldBeTrue();
            urlConvention.GetUrlsContext.ActionMethod.ShouldEqual(actionMethod);
            urlConvention.GetUrlsContext.HttpMethod.ShouldEqual("GET");
            urlConvention.GetUrlsContext.MethodSegments
                .ToUrl().ShouldEqual("Segment");
            urlConvention.GetUrlsContext.UrlParameters.ShouldBeEmpty();
            urlConvention.GetUrlsContext.Parameters.ShouldBeEmpty();
            urlConvention.GetUrlsContext.RequestParameter.ShouldEqual(null);
            urlConvention.GetUrlsContext.ResponseType.ShouldEqual(null);
        }

        [Test]
        public void Should_not_use_url_conventions_where_the_instance_does_not_apply()
        {
            var urlConvention = new TestUrlConvention
            {
                AppliesToFunc = c => false,
                GetUrlsFunc = c => new[] { "fark", "farker" }
            };
            _urlConventions.Add(urlConvention);
            _configuration.UrlConventions.Configure(x => x.Append(urlConvention));

            var descriptors = _routeConvention.GetRouteDescriptors(new RouteContext(
                ActionMethod.From<UrlConventionHandler>(x => x.Get_Segment())));

            descriptors.Count.ShouldEqual(1);

            urlConvention.AppliesToCalled.ShouldBeTrue();
            urlConvention.GetUrlsCalled.ShouldBeFalse();
        }

        [Test]
        public void Should_not_use_url_conventions_where_does_not_apply_in_configuration()
        {
            var urlConvention = new TestUrlConvention
            {
                GetUrlsFunc = c => new[] { "fark", "farker" }
            };
            _urlConventions.Add(urlConvention);
            _configuration.UrlConventions.Configure(c => c.Append<TestUrlConvention>(x => false));

            var descriptors = _routeConvention.GetRouteDescriptors(new RouteContext(
                ActionMethod.From<UrlConventionHandler>(x => x.Get_Segment())));

            descriptors.Count.ShouldEqual(1);

            urlConvention.AppliesToCalled.ShouldBeFalse();
            urlConvention.GetUrlsCalled.ShouldBeFalse();
        }

        [Test]
        public void Should_not_use_url_conventions_if_no_plugin_definition_found()
        {
            var urlConvention = new TestUrlConvention
            {
                GetUrlsFunc = c => new[] { "fark", "farker" }
            };
            _urlConventions.Add(urlConvention);

            var descriptors = _routeConvention.GetRouteDescriptors(new RouteContext(
                ActionMethod.From<UrlConventionHandler>(x => x.Get_Segment())));

            descriptors.Count.ShouldEqual(1);

            urlConvention.AppliesToCalled.ShouldBeFalse();
            urlConvention.GetUrlsCalled.ShouldBeFalse();
        }

        public class ParseHttpMethod
        {
            public void GetFark() { }
            public void Get() { }
            public void Post() { }
            public void Put() { }
            public void Delete() { }
            public void Options() { }
            public void Head() { }
            public void Trace() { }
            public void Connect() { }
        }

        [TestCase(nameof(ParseHttpMethod.GetFark), "GET")]
        [TestCase(nameof(ParseHttpMethod.Get), "GET")]
        [TestCase(nameof(ParseHttpMethod.Post), "POST")]
        [TestCase(nameof(ParseHttpMethod.Put), "PUT")]
        [TestCase(nameof(ParseHttpMethod.Delete), "DELETE")]
        [TestCase(nameof(ParseHttpMethod.Options), "OPTIONS")]
        [TestCase(nameof(ParseHttpMethod.Head), "HEAD")]
        [TestCase(nameof(ParseHttpMethod.Trace), "TRACE")]
        [TestCase(nameof(ParseHttpMethod.Connect), "CONNECT")]
        public void Should_return_method_name_from_method_name_convention(string name, string expected)
        {
            DefaultRouteConvention.DefaultHttpMethodConvention(_configuration,
                    ActionMethod.From<ParseHttpMethod>(name))
                .ShouldEqual(expected);
        }

        public class ParseSegments
        {
            public void Get() { }
            public void GetFark() { }
            public void GetFark_Farker() { }
        }

        [TestCase(nameof(ParseSegments.GetFark), "Fark")]
        [TestCase(nameof(ParseSegments.Get), null)]
        [TestCase(nameof(ParseSegments.GetFark_Farker), "Fark,Farker")]
        public void Should_return_segments_from_segment_convention(
            string name, string expected)
        {
            var result = DefaultRouteConvention.DefaultActionSegmentsConvention(_configuration,
                ActionMethod.From<ParseSegments>(name));

            if (expected == null) result.ShouldBeNull();
            else result.ShouldOnlyContain(expected.Split(','));
        }

    }
}
