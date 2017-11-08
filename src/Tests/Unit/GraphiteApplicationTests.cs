using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Routing;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Binding;
using Graphite.DependencyInjection;
using Graphite.Diagnostics;
using Graphite.Http;
using Graphite.Readers;
using Graphite.Reflection;
using Graphite.Routing;
using Graphite.StructureMap;
using Graphite.Writers;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Common.Fakes;

namespace Tests.Unit
{
    [TestFixture]
    public class GraphiteApplicationTests
    {
        private Container _container;
        private GraphiteApplication _application;

        [SetUp]
        public void Setup()
        {
            _container = new Container();
            RegisterRequestObjects(_container);
            _container.Register(Substitute.For<IRequestPropertiesProvider>());
            _application = new GraphiteApplication(new HttpConfiguration());
        }

        [Test]
        public void Should_call_configured_initializer_instance()
        {
            var initializer = Substitute.For<IInitializer>();

            _application.Initialize(c => c
                .IncludeThisAssembly()
                .UseContainer(_container)
                .FilterHandlersBy((a, t) => false)
                .WithInitializer(initializer)
            );

            initializer.Received(1).Initialize();
        }

        [Test]
        public void Should_initialize_graphite()
        {
            _application.Initialized.ShouldBeFalse();

            _application.Initialize(c => c
                .IncludeTypeAssembly<GraphiteApplicationTests>()
                .UseContainer(_container)
                .FilterHandlersBy((a, t) => false)
                .ConfigureActionDecorators(x => x.Append<TestActionDecorator>()));

            _application.Initialized.ShouldBeTrue();
            _application.Container.ShouldNotBeNull();
            _application.Metrics.ShouldNotBeNull();

            Should_be_configured_with_graphite_defaults(_container);
        }

        [Test]
        public void Should_fail_if_no_assemblies_specified()
        {
            _application.Should().Throw<GraphiteException>(
                x => x.Initialize(c => c.UseContainer(_container)));
        }

        public static void RegisterRequestObjects(Container container)
        {
            var requestGraph = RequestGraph.Create();
            container.Register(requestGraph.GetActionDescriptor());
            container.Register(requestGraph.GetRouteDescriptor());
            container.Register(requestGraph.GetHttpRequestMessage());
            container.Register(requestGraph.GetHttpResponseMessage());
            container.Register(requestGraph.ActionMethod);
            container.Register(requestGraph.GetUrlParameters());
            container.Register(requestGraph.GetQuerystringParameters());
        }

        public static void Should_be_configured_with_graphite_defaults(Container container)
        {
            var configuration = container.GetInstance<Configuration>();

            configuration.Container.ShouldEqual(container);
            configuration.Assemblies.ShouldOnlyContain(Assembly.GetExecutingAssembly());

            var actionMethodSources = container.GetInstances<IActionMethodSource>().ToList();
            actionMethodSources.Count.ShouldEqual(1);
            actionMethodSources[0].ShouldBeType<DefaultActionMethodSource>();

            var actionSources = container.GetInstances<IActionSource>().ToList();
            actionSources.Count.ShouldEqual(2);
            actionSources[0].ShouldBeType<DiagnosticsActionSource>();
            actionSources[1].ShouldBeType<DefaultActionSource>();

            var actionDecorators = container.GetInstances<IActionDecorator>().ToList();
            actionDecorators.Count.ShouldEqual(1);
            actionDecorators[0].ShouldBeType<TestActionDecorator>();

            var urlConventions = container.GetInstances<IUrlConvention>().ToList();
            urlConventions.Count.ShouldEqual(1);
            urlConventions[0].ShouldBeType<DefaultUrlConvention>();

            var requestReaders = container.GetInstances<IRequestReader>().ToList();
            requestReaders.Count.ShouldEqual(6);
            requestReaders[0].ShouldBeType<StringReader>();
            requestReaders[1].ShouldBeType<StreamReader>();
            requestReaders[2].ShouldBeType<ByteReader>();
            requestReaders[3].ShouldBeType<JsonReader>();
            requestReaders[4].ShouldBeType<XmlReader>();
            requestReaders[5].ShouldBeType<FormReader>();

            var requestBinders = container.GetInstances<IRequestBinder>().ToList();
            requestBinders.Count.ShouldEqual(11);
            requestBinders[0].ShouldBeType<MultipartFormBinder>();
            requestBinders[1].ShouldBeType<ReaderBinder>();
            requestBinders[2].ShouldBeType<UrlParameterBinder>();
            requestBinders[3].ShouldBeType<QuerystringBinder>();
            requestBinders[4].ShouldBeType<FormBinder>();
            requestBinders[5].ShouldBeType<JsonBinder>();
            requestBinders[6].ShouldBeType<XmlBinder>();
            requestBinders[7].ShouldBeType<HeaderBinder>();
            requestBinders[8].ShouldBeType<CookieBinder>();
            requestBinders[9].ShouldBeType<RequestPropertiesBinder>();
            requestBinders[10].ShouldBeType<ContainerBinder>();

            var valueMappers = container.GetInstances<IValueMapper>().ToList();
            valueMappers.Count.ShouldEqual(1);
            valueMappers[0].ShouldBeType<SimpleTypeMapper>();

            var responseWriters = container.GetInstances<IResponseWriter>().ToList();
            responseWriters.Count.ShouldEqual(6);
            responseWriters[0].ShouldBeType<RedirectWriter>();
            responseWriters[1].ShouldBeType<StringWriter>();
            responseWriters[2].ShouldBeType<StreamWriter>();
            responseWriters[3].ShouldBeType<ByteWriter>();
            responseWriters[4].ShouldBeType<JsonWriter>();
            responseWriters[5].ShouldBeType<XmlWriter>();

            var responseStatus = container.GetInstances<IResponseStatus>().ToList();
            responseStatus.Count.ShouldEqual(1);
            responseStatus[0].ShouldBeType<DefaultResponseStatus>();

            container.GetInstance<IContainer>().ShouldBeType<TrackingContainer>();
            container.GetInstance<Configuration>().ShouldEqual(configuration);
            container.GetInstance<IRouteConvention>().ShouldBeType<DefaultRouteConvention>();
            container.GetInstance<IInlineConstraintBuilder>().ShouldBeType<DefaultInlineConstraintBuilder>();
            container.GetInstance<ITypeCache>().ShouldBeType<TypeCache>();
            container.GetInstance<IInitializer>().ShouldBeType<Initializer>();
            container.GetInstance<IHttpRouteMapper>().ShouldBeType<HttpRouteMapper>();
            container.GetInstance<IInlineConstraintResolver>().ShouldBeType<DefaultInlineConstraintResolver>();
            container.GetInstance<IBehaviorChainInvoker>().ShouldBeType<BehaviorChainInvoker>();
            container.GetInstance<IActionInvoker>().ShouldBeType<ActionInvoker>();
        }
    }
}
