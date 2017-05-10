using System;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Graphite;
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
                    .ExcludeTypeNamespace<Global>()
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
}