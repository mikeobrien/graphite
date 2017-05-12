using System;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class RequestInfoBinderTests
    {
        private const string ActionMethod = "actionMethod";
        private const string RemoteAddress = "remoteAddress";
        private const string RemoteAddressValue = "192.168.1.1";
        private const string RemotePort = "remotePort";
        private const int RemotePortValue = 80;

        public class Handler
        {
            public void ByType(ActionMethod actionMethod) { }
            public void ByType(RouteDescriptor actionMethod) { }
            public void ByType(UrlParameters actionMethod) { }
            public void ByType(QuerystringParameters actionMethod) { }
            public void ByType(HttpRequestMessage actionMethod) { }
            public void ByType(HttpConfiguration actionMethod) { }
            public void ByType(HttpRequestContext actionMethod) { }
            public void ByAttribute([FromRequestInfo] ActionMethod actionMethod) { }
            public void ByTypeWithFromUriAttribute([FromUri] ActionMethod actionMethod) { }
            public void ByTypeWithFromBodyAttribute([FromBody] ActionMethod actionMethod) { }

            public void ByName(string remoteAddress) { }
            public void ByName(int remotePort) { }
            public void ByAttribute([FromRequestInfo] string remoteAddress) { }
            public void ByAttributeName([FromRequestInfo(RemoteAddress)] string sourceIp) { }
            public void ByNameWithFromUriAttribute([FromUri] string remoteAddress) { }
            public void ByNameWithFromBodyAttribute([FromBody] string remoteAddress) { }
        }

        public static object[][] BindingCases = TestCaseSource
            .CreateWithExpression<Handler, string, BindingMode, bool, Type, object>(x => x
                .Add(h => h.ByType((ActionMethod)null), ActionMethod, BindingMode.Implicit, false, typeof(ActionMethod), null)
                .Add(h => h.ByType((RouteDescriptor)null), ActionMethod, BindingMode.Implicit, false, typeof(RouteDescriptor), null)
                .Add(h => h.ByType((UrlParameters)null), ActionMethod, BindingMode.Implicit, false, typeof(UrlParameters), null)
                .Add(h => h.ByType((QuerystringParameters)null), ActionMethod, BindingMode.Implicit, false, typeof(QuerystringParameters), null)
                .Add(h => h.ByType((HttpRequestMessage)null), ActionMethod, BindingMode.Implicit, false, typeof(HttpRequestMessage), null)
                .Add(h => h.ByType((HttpConfiguration)null), ActionMethod, BindingMode.Implicit, false, typeof(HttpConfiguration), null)
                .Add(h => h.ByType((HttpRequestContext)null), ActionMethod, BindingMode.Implicit, false, typeof(HttpRequestContext), null)
                .Add(h => h.ByType((ActionMethod)null), ActionMethod, BindingMode.Explicit, false, null, null)
                .Add(h => h.ByType((ActionMethod)null), ActionMethod, BindingMode.None, false, null, null)
                .Add(h => h.ByType((ActionMethod)null), ActionMethod, BindingMode.Convention, false, null, null)

                .Add(h => h.ByAttribute((ActionMethod)null), ActionMethod, BindingMode.Implicit, false, typeof(ActionMethod), null)
                .Add(h => h.ByAttribute((ActionMethod)null), ActionMethod, BindingMode.Explicit, false, typeof(ActionMethod), null)
                .Add(h => h.ByAttribute((ActionMethod)null), ActionMethod, BindingMode.None, false, null, null)
                .Add(h => h.ByAttribute((ActionMethod)null), ActionMethod, BindingMode.Convention, false, null, null)

                .Add(h => h.ByTypeWithFromUriAttribute(null), ActionMethod, BindingMode.Implicit, false, null, null)
                .Add(h => h.ByTypeWithFromUriAttribute(null), ActionMethod, BindingMode.Explicit, false, null, null)
                .Add(h => h.ByTypeWithFromUriAttribute(null), ActionMethod, BindingMode.None, false, null, null)
                .Add(h => h.ByTypeWithFromUriAttribute(null), ActionMethod, BindingMode.Convention, false, null, null)

                .Add(h => h.ByTypeWithFromBodyAttribute(null), ActionMethod, BindingMode.Implicit, false, null, null)
                .Add(h => h.ByTypeWithFromBodyAttribute(null), ActionMethod, BindingMode.Explicit, false, null, null)
                .Add(h => h.ByTypeWithFromBodyAttribute(null), ActionMethod, BindingMode.None, false, null, null)
                .Add(h => h.ByTypeWithFromBodyAttribute(null), ActionMethod, BindingMode.Convention, false, null, null)

                .Add(h => h.ByName(null), RemoteAddress, BindingMode.Implicit, false, null, RemoteAddressValue)
                .Add(h => h.ByName(0), RemotePort, BindingMode.Implicit, false, null, RemotePortValue)
                .Add(h => h.ByName(null), RemoteAddress, BindingMode.Implicit, true, null, RemoteAddressValue)
                .Add(h => h.ByName(0), RemotePort, BindingMode.Implicit, true, null, RemotePortValue)
                .Add(h => h.ByName(null), RemoteAddress, BindingMode.Explicit, false, null, null)
                .Add(h => h.ByName(null), RemoteAddress, BindingMode.None, false, null, null)
                .Add(h => h.ByName(null), RemoteAddress, BindingMode.Convention, false, null, null)

                .Add(h => h.ByAttribute((string)null), RemoteAddress, BindingMode.Implicit, false, null, RemoteAddressValue)
                .Add(h => h.ByAttribute((string)null), RemoteAddress, BindingMode.Explicit, false, null, RemoteAddressValue)
                .Add(h => h.ByAttribute((string)null), RemoteAddress, BindingMode.None, false, null, null)
                .Add(h => h.ByAttribute((string)null), RemoteAddress, BindingMode.Convention, false, null, null)

                .Add(h => h.ByAttributeName(null), "sourceIp", BindingMode.Implicit, false, null, RemoteAddressValue)
                .Add(h => h.ByAttributeName(null), "sourceIp", BindingMode.Explicit, false, null, RemoteAddressValue)
                .Add(h => h.ByAttributeName(null), "sourceIp", BindingMode.None, false, null, null)
                .Add(h => h.ByAttributeName(null), "sourceIp", BindingMode.Convention, false, null, null)

                .Add(h => h.ByNameWithFromUriAttribute(null), RemoteAddress, BindingMode.Implicit, false, null, null)
                .Add(h => h.ByNameWithFromUriAttribute(null), RemoteAddress, BindingMode.Explicit, false, null, null)
                .Add(h => h.ByNameWithFromUriAttribute(null), RemoteAddress, BindingMode.None, false, null, null)
                .Add(h => h.ByNameWithFromUriAttribute(null), RemoteAddress, BindingMode.Convention, false, null, null)

                .Add(h => h.ByNameWithFromBodyAttribute(null), RemoteAddress, BindingMode.Implicit, false, null, null)
                .Add(h => h.ByNameWithFromBodyAttribute(null), RemoteAddress, BindingMode.Explicit, false, null, null)
                .Add(h => h.ByNameWithFromBodyAttribute(null), RemoteAddress, BindingMode.None, false, null, null)
                .Add(h => h.ByNameWithFromBodyAttribute(null), RemoteAddress, BindingMode.Convention, false, null, null)
            );

        [TestCaseSource(nameof(BindingCases))]
        public async Task Should_bind_request_info(Expression<Action<Handler>> action,
            string parameter, BindingMode bindingMode, bool selfHosted, 
            Type expectedType, object expectedValue)
        {
            var requestGraph = RequestGraph
                .CreateFor(action)
                .AddParameter(parameter)
                .AddValueMapper(new SimpleTypeMapper());

            if (selfHosted)
            {
                requestGraph.AddRequestProperty(RemoteEndpointMessageProperty.Name, 
                    new RemoteEndpointMessageProperty(RemoteAddressValue, 80));
            }
            else
            {
                AddHttpServerVariables(requestGraph, new NameValueCollection
                {
                    { "REMOTE_ADDR", RemoteAddressValue },
                    { "REMOTE_PORT", RemotePortValue.ToString() }
                });
            }

            requestGraph.Configuration.RequestInfoBindingMode = bindingMode;

            await new RequestInfoBinder(requestGraph.ValueMappers)
                .Bind(requestGraph.GetRequestBinderContext());

            if (expectedType != null)
            {
                requestGraph.ActionArguments[0].ShouldNotBeNull();
                requestGraph.ActionArguments[0].ShouldBeType(expectedType);
            }
            else requestGraph.ActionArguments[0].ShouldEqual(expectedValue);
        }

        private void AddHttpServerVariables(RequestGraph requestGraph, NameValueCollection serverVariables)
        {
            var httpContext = Substitute.For<HttpContextBase>();
            var httpRequest = Substitute.For<HttpRequestBase>();

            httpContext.Request.Returns(httpRequest);
            httpRequest.ServerVariables.Returns(serverVariables);

            requestGraph.AddRequestProperty(RequestInfoBinder.HttpContextKey, httpContext);
        }

        public class MappingHandler
        {
            public void Action(string param1, string param2, string param3) { }
        }

        [Test]
        public async Task Should_use_the_first_mapper_that_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                    .AddAllActionParameters()
                    .Configure(x => x.BindRequestInfo())
                    .AddValueMapper1(x => x.Values.First() + "mapper1")
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            AddHttpServerVariables(requestGraph, new NameValueCollection
            {
                { "param2", "value2" },
                { "param3", "value3" }
            });

            var binder = new RequestInfoBinder(requestGraph.ValueMappers);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value2mapper1", "value3mapper1");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeTrue();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeFalse();
        }

        [Test]
        public async Task Should_not_use_a_mapper_that_doesnt_apply_in_configuration()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                .AddAllActionParameters()
                .Configure(x => x.BindRequestInfo())
                    .AddValueMapper1(x => x.Values.First() + "mapper1", configAppliesTo: x => false)
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            AddHttpServerVariables(requestGraph, new NameValueCollection
            {
                { "param2", "value2" },
                { "param3", "value3" }
            });

            var binder = new RequestInfoBinder(requestGraph.ValueMappers);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value2mapper2", "value3mapper2");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeFalse();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_use_a_mapper_that_doesnt_apply_at_runtime()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                .AddAllActionParameters()
                .Configure(x => x.BindRequestInfo())
                    .AddValueMapper1(x => x.Values.First() + "mapper1", instanceAppliesTo: x => false)
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            AddHttpServerVariables(requestGraph, new NameValueCollection
            {
                { "param2", "value2" },
                { "param3", "value3" }
            });

            var binder = new RequestInfoBinder(requestGraph.ValueMappers);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value2mapper2", "value3mapper2");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_bind_if_no_mappers_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                .AddAllActionParameters()
                .Configure(x => x.BindRequestInfo());

            AddHttpServerVariables(requestGraph, new NameValueCollection
            {
                { "param2", "value2" },
                { "param3", "value3" }
            });

            var binder = new RequestInfoBinder(requestGraph.ValueMappers);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        [Test]
        public async Task Should_map_null_if_there_are_no_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                .Configure(x => x.BindRequestInfo());

            AddHttpServerVariables(requestGraph, new NameValueCollection
            {
                { "param2", "value2" },
                { "param3", "value3" }
            });

            var binder = new RequestInfoBinder(requestGraph.ValueMappers);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }
    }
}
