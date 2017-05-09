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
    public class GraphiteBootstrapTests
    {
        [SetUp, TearDown]
        public void Setup()
        {
            GlobalConfiguration.Configuration.Clear();
        }

        [Test]
        public void Should_call_configured_initializer_instance()
        {
            var container = new Container();
            var initializer = Substitute.For<IInitializer>();

            GlobalConfiguration.Configure(httpConfig =>
            {
                httpConfig.InitializeGraphite(config => config
                    .UseContainer(container)
                    .FilterHandlersBy((c, t) => false)
                    .WithInitializer(initializer)
                );
            });
            
            initializer.Received(1).Initialize();
        }

        [Test]
        public void Should_initialize_graphite_with_defaults()
        {
            var container = new Container();

            GlobalConfiguration.Configuration.InitializeGraphite(config => 
                config
                    .UseContainer(container)
                    .FilterHandlersBy((c, t) => false)
                    .ConfigureActionDecorators(x => x.Append<TestActionDecorator>())
            );
            
            Should_be_configured_with_graphite_defaults(container);
        }

        [Test]
        public void Should_initialize_graphite_in_configuration_dsl_with_defaults()
        {
            var container = new Container();

            GlobalConfiguration.Configure(httpConfig =>
            {
                httpConfig.InitializeGraphite(config => {
                    config
                        .IncludeTypeAssembly<GraphiteBootstrapTests>()
                        .UseContainer(container)
                        .FilterHandlersBy((c, t) => false)
                        .ConfigureActionDecorators(x => x.Append<TestActionDecorator>());
                });
            });
            
            Should_be_configured_with_graphite_defaults(container);
        }

        public void Should_be_configured_with_graphite_defaults(Container container)
        {
            // Register per request instances
            var requestGraph = RequestGraph.Create();
            container.Register(requestGraph.GetRouteDescriptor());
            container.Register(requestGraph.GetHttpRequestMessage());
            container.Register(requestGraph.GetHttpResponseMessage());
            container.Register(requestGraph.ActionMethod);
            container.Register(requestGraph.GetUrlParameters());
            container.Register(requestGraph.GetQuerystringParameters());

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
            urlConventions.Count.ShouldEqual(2);
            urlConventions[0].ShouldBeType<DefaultUrlConvention>();
            urlConventions[1].ShouldBeType<AliasUrlConvention>();

            var requestReaders = container.GetInstances<IRequestReader>().ToList();
            requestReaders.Count.ShouldEqual(6);
            requestReaders[0].ShouldBeType<StringReader>();
            requestReaders[1].ShouldBeType<StreamReader>();
            requestReaders[2].ShouldBeType<ByteReader>();
            requestReaders[3].ShouldBeType<JsonReader>();
            requestReaders[4].ShouldBeType<XmlReader>();
            requestReaders[5].ShouldBeType<FormReader>();

            var requestBinders = container.GetInstances<IRequestBinder>().ToList();
            requestBinders.Count.ShouldEqual(10);
            requestBinders[0].ShouldBeType<ReaderBinder>();
            requestBinders[1].ShouldBeType<UrlParameterBinder>();
            requestBinders[2].ShouldBeType<QuerystringBinder>();
            requestBinders[3].ShouldBeType<FormBinder>();
            requestBinders[4].ShouldBeType<JsonBinder>();
            requestBinders[5].ShouldBeType<XmlBinder>();
            requestBinders[6].ShouldBeType<HeaderBinder>();
            requestBinders[7].ShouldBeType<CookieBinder>();
            requestBinders[8].ShouldBeType<RequestInfoBinder>();
            requestBinders[9].ShouldBeType<ContainerBinder>();

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

            container.GetInstance<IContainer>().ShouldBeType<TrackingContainer>();
            container.GetInstance<Configuration>().ShouldEqual(configuration);
            container.GetInstance<IRouteConvention>().ShouldBeType<DefaultRouteConvention>();
            container.GetInstance<IInlineConstraintBuilder>().ShouldBeType<DefaultInlineConstraintBuilder>();
            container.GetInstance<ITypeCache>().ShouldBeType<TypeCache>();
            container.GetInstance<IInitializer>().ShouldBeType<Initializer>();
            container.GetInstance<IRouteMapper>().ShouldBeType<RouteMapper>();
            container.GetInstance<IInlineConstraintResolver>().ShouldBeType<DefaultInlineConstraintResolver>();
            container.GetInstance<IUnhandledExceptionHandler>().ShouldBeType<UnhandledExceptionHandler>();
            container.GetInstance<IBehaviorChainInvoker>().ShouldBeType<BehaviorChainInvoker>();
            container.GetInstance<IActionInvoker>().ShouldBeType<ActionInvoker>();
        }
    }
}
