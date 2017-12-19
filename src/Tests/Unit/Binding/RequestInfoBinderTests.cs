using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Http;
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
            public void ByAttribute([FromRequestProperties] ActionMethod actionMethod) { }
            public void ByTypeWithFromUriAttribute([FromUri] ActionMethod actionMethod) { }
            public void ByTypeWithFromBodyAttribute([FromBody] ActionMethod actionMethod) { }

            public void ByName(string remoteAddress) { }
            public void ByName(int remotePort) { }
            public void ByAttribute([FromRequestProperties] string remoteAddress) { }
            public void ByAttributeName([FromRequestProperties(RemoteAddress)] string sourceIp) { }
            public void ByNameWithFromUriAttribute([FromUri] string remoteAddress) { }
            public void ByNameWithFromBodyAttribute([FromBody] string remoteAddress) { }
        }

        private Configuration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
        }

        public static object[][] BindingCases = TestCaseSource
            .CreateWithExpression<Handler, string, BindingMode, Type, object>(x => x
            
                .Add(h => h.ByAttribute((ActionMethod)null), ActionMethod, BindingMode.None, null, null)
                .Add(h => h.ByAttribute((ActionMethod)null), ActionMethod, BindingMode.Convention, null, null)

                .Add(h => h.ByTypeWithFromUriAttribute(null), ActionMethod, BindingMode.Implicit, null, null)
                .Add(h => h.ByTypeWithFromUriAttribute(null), ActionMethod, BindingMode.Explicit, null, null)
                .Add(h => h.ByTypeWithFromUriAttribute(null), ActionMethod, BindingMode.None, null, null)
                .Add(h => h.ByTypeWithFromUriAttribute(null), ActionMethod, BindingMode.Convention, null, null)

                .Add(h => h.ByTypeWithFromBodyAttribute(null), ActionMethod, BindingMode.Implicit, null, null)
                .Add(h => h.ByTypeWithFromBodyAttribute(null), ActionMethod, BindingMode.Explicit, null, null)
                .Add(h => h.ByTypeWithFromBodyAttribute(null), ActionMethod, BindingMode.None, null, null)
                .Add(h => h.ByTypeWithFromBodyAttribute(null), ActionMethod, BindingMode.Convention, null, null)

                .Add(h => h.ByName(null), RemoteAddress, BindingMode.Implicit, null, RemoteAddressValue)
                .Add(h => h.ByName(0), RemotePort, BindingMode.Implicit, null, RemotePortValue)
                .Add(h => h.ByName(null), RemoteAddress, BindingMode.Explicit, null, null)
                .Add(h => h.ByName(null), RemoteAddress, BindingMode.None, null, null)
                .Add(h => h.ByName(null), RemoteAddress, BindingMode.Convention, null, null)

                .Add(h => h.ByAttribute((string)null), RemoteAddress, BindingMode.Implicit, null, RemoteAddressValue)
                .Add(h => h.ByAttribute((string)null), RemoteAddress, BindingMode.Explicit, null, RemoteAddressValue)
                .Add(h => h.ByAttribute((string)null), RemoteAddress, BindingMode.None, null, null)
                .Add(h => h.ByAttribute((string)null), RemoteAddress, BindingMode.Convention, null, null)

                .Add(h => h.ByAttributeName(null), "sourceIp", BindingMode.Implicit, null, RemoteAddressValue)
                .Add(h => h.ByAttributeName(null), "sourceIp", BindingMode.Explicit, null, RemoteAddressValue)
                .Add(h => h.ByAttributeName(null), "sourceIp", BindingMode.None, null, null)
                .Add(h => h.ByAttributeName(null), "sourceIp", BindingMode.Convention, null, null)

                .Add(h => h.ByNameWithFromUriAttribute(null), RemoteAddress, BindingMode.Implicit, null, null)
                .Add(h => h.ByNameWithFromUriAttribute(null), RemoteAddress, BindingMode.Explicit, null, null)
                .Add(h => h.ByNameWithFromUriAttribute(null), RemoteAddress, BindingMode.None, null, null)
                .Add(h => h.ByNameWithFromUriAttribute(null), RemoteAddress, BindingMode.Convention, null, null)

                .Add(h => h.ByNameWithFromBodyAttribute(null), RemoteAddress, BindingMode.Implicit, null, null)
                .Add(h => h.ByNameWithFromBodyAttribute(null), RemoteAddress, BindingMode.Explicit, null, null)
                .Add(h => h.ByNameWithFromBodyAttribute(null), RemoteAddress, BindingMode.None, null, null)
                .Add(h => h.ByNameWithFromBodyAttribute(null), RemoteAddress, BindingMode.Convention, null, null)
            );

        [TestCaseSource(nameof(BindingCases))]
        public async Task Should_bind_request_info(Expression<Action<Handler>> action,
            string parameter, BindingMode bindingMode, Type expectedType, object expectedValue)
        {
            var requestGraph = RequestGraph
                .CreateFor(action)
                .AddParameters(parameter)
                .AddValueMapper(new SimpleTypeMapper(new ParsedValueMapper()));

            var properties = new Dictionary<string, object>
            {
                ["REMOTEADDRESS"] = RemoteAddressValue,
                ["REMOTEPORT"] = RemotePortValue.ToString()
            };

            requestGraph.Configuration.RequestInfoBindingMode = bindingMode;

            await CreateBinder(requestGraph, properties)
                .Bind(requestGraph.GetRequestBinderContext());

            if (expectedType != null)
            {
                requestGraph.ActionArguments[0].ShouldNotBeNull();
                requestGraph.ActionArguments[0].ShouldBeType(expectedType);
            }
            else requestGraph.ActionArguments[0].ShouldEqual(expectedValue);
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
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"))
                    .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var properties = new Dictionary<string, object>
            {
                ["param2"] = "value2",
                ["param3"] = "value3"
            };

            var binder = CreateBinder(requestGraph, properties);

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
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"), 
                        configAppliesTo: x => false)
                    .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var properties = new Dictionary<string, object>
            {
                ["param2"] = "value2",
                ["param3"] = "value3"
            };

            var binder = CreateBinder(requestGraph, properties);

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
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"), 
                        instanceAppliesTo: x => false)
                    .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var properties = new Dictionary<string, object>
            {
                ["param2"] = "value2",
                ["param3"] = "value3"
            };

            var binder = CreateBinder(requestGraph, properties);

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

            var properties = new Dictionary<string, object>
            {
                ["param2"] = "value2",
                ["param3"] = "value3"
            };

            var binder = CreateBinder(requestGraph, properties);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        [Test]
        public async Task Should_map_null_if_there_are_no_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                .Configure(x => x.BindRequestInfo());

            var properties = new Dictionary<string, object>
            {
                ["param2"] = "value2",
                ["param3"] = "value3"
            };

            var binder = CreateBinder(requestGraph, properties);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        public RequestPropertiesBinder CreateBinder(RequestGraph requestGraph, 
            IDictionary<string, object> parameters)
        {
            var propertyProvider = Substitute.For<IRequestPropertiesProvider>();
            propertyProvider.GetProperties().Returns(parameters);
            return new RequestPropertiesBinder(
                requestGraph.Configuration,
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetArgumentBinder(),
                propertyProvider);
        }
    }
}
