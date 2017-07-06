using System.Web.Http;
using Graphite.Owin;
using Graphite.Hosting;
using Graphite.StructureMap;
using Graphite.DependencyInjection;
using Graphite.Http;
using Microsoft.Owin.Builder;
using NUnit.Framework;
using Should;
using Tests.Common.Fakes;

namespace Tests.Unit.Owin
{
    [TestFixture]
    public class BootstrapTests
    {
        private AppBuilder _builder;
        private Container _container;

        [SetUp]
        public void Setup()
        {
            _builder = new AppBuilder();
            _container = new Container();
            GraphiteApplicationTests.RegisterRequestObjects(_container);
        }

        [Test]
        public void Should_initialize_graphite_with_defaults()
        {
            _builder.InitializeGraphite(config =>
                config
                    .UseContainer(_container)
                    .FilterHandlersBy((c, t) => false)
                    .ConfigureActionDecorators(x => x.Append<TestActionDecorator>())
            );

            GraphiteApplicationTests.Should_be_configured_with_graphite_defaults(_container);
            Should_be_configured_with_graphite_defaults(_container);
        }

        public static void Should_be_configured_with_graphite_defaults(Container container)
        {
            container.GetInstance<IRequestPropertiesProvider>().ShouldBeType<OwinRequestPropertyProvider>();
            container.GetInstance<IPathProvider>().ShouldBeType<OwinPathProvider>();
        }
    }
}
