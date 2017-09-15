using System.Web.Http;
using Graphite.AspNet;
using Graphite.DependencyInjection;
using Graphite.Hosting;
using Graphite.Http;
using Graphite.StructureMap;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Common.Fakes;

namespace Tests.Unit.AspNet
{
    [TestFixture]
    public class BootstrapTests
    {
        private Container _container;

        [SetUp]
        public void Setup()
        {
            _container = new Container();
            GraphiteApplicationTests.RegisterRequestObjects(_container);
            GlobalConfiguration.Configuration.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            GlobalConfiguration.Configuration.Clear();
        }

        [Test]
        public void Should_initialize_graphite_with_defaults()
        {
            GlobalConfiguration.Configuration.InitializeGraphite(config =>
                config
                    .IncludeThisAssembly()
                    .UseContainer(_container)
                    .FilterHandlersBy((c, t) => false)
                    .ConfigureActionDecorators(x => x.Append<TestActionDecorator>())
            );

            GraphiteApplicationTests.Should_be_configured_with_graphite_defaults(_container);
            Should_be_configured_with_graphite_defaults(_container);
        }

        [Test]
        public void Should_initialize_graphite_in_configuration_dsl_with_defaults()
        {
            GlobalConfiguration.Configure(httpConfig =>
            {
                httpConfig.InitializeGraphite(config => {
                    config
                        .IncludeTypeAssembly<GraphiteApplicationTests>()
                        .UseContainer(_container)
                        .FilterHandlersBy((c, t) => false)
                        .ConfigureActionDecorators(x => x.Append<TestActionDecorator>());
                });
            });

            Should_be_configured_with_graphite_defaults(_container);
            GraphiteApplicationTests.Should_be_configured_with_graphite_defaults(_container);
        }

        public static void Should_be_configured_with_graphite_defaults(Container container)
        {
            container.GetInstance<IRequestPropertiesProvider>().ShouldBeType<AspNetRequestPropertyProvider>();
            container.GetInstance<IPathProvider>().ShouldBeType<AspNetPathProvider>();
        }
    }
}
