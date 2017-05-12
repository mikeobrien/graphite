using System;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Graphite;
using Graphite.Actions;
using Graphite.StructureMap;

namespace TestHarness
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.RouteExistingFiles = true;

            var configuration = GlobalConfiguration.Configuration;
            configuration.MapHttpAttributeRoutes();

            configuration
                .InitializeGraphite(c => c
                    .EnableDiagnosticsInDebugMode()
                    .UseStructureMapContainer<Registry>(configuration)
                    .ExcludeTypeNamespaceFromUrl<Global>()
                    .ConfigureActionDecorators(d => d.Append<TestActionDecorator>())
                    .ConfigureBehaviors(b => b
                        .Append<Behavior1>()
                        .Append<Behavior2>()
                        .Append<Behavior3>()
                        .Append<Behavior4>()
                        .Append<Behavior5>()
                        .Append<Behavior6>()
                        .Append<Behavior7>()
                        .Append<Behavior8>()
                        .Append<Behavior9>()
                        .Append<Behavior10>()));

            configuration.EnsureInitialized();
        }
    }

    public class TestActionDecorator : IActionDecorator
    {
        public bool AppliesTo(ActionDecoratorContext context)
        {
            return context.ActionDescriptor.Route.Method == "GET";
        }

        public void Decorate(ActionDecoratorContext context)
        {
            context.ActionDescriptor.Registry.Register<IDependency, Dependency>();
        }
    }

    public interface IDependency { }
    public class Dependency : IDependency { }
}