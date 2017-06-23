using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Graphite;
using Graphite.Actions;
using Graphite.Authentication;
using Graphite.Behaviors;
using Graphite.StructureMap;
using TestHarness.Action;
using TestHarness.Routing;

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
                    //.UseBender(x => x.UseCamelCaseNaming())
                    //.ConfigureJsonNet(x => x.ContractResolver = 
                    //    new CamelCasePropertyNamesContractResolver())
                    .UseStructureMapContainer<Registry>(configuration)
                    .ExcludeTypeNamespaceFromUrl<Global>()
                    .ConfigureActionDecorators(d => d.Append<TestActionDecorator>())
                    .BindCookies()
                    .BindRequestInfo()
                    .BindHeaders()
                    .BindContainer()
                    .ConfigureHttpRouteDecorators(x => x
                        .Append<RoutingTestHandler.HttpRouteDecorator>(d => d
                            .ActionMethod.Name.Contains("Decorator")))
                    .ConfigureAuthenticators(x => x
                        .Append<TestBearerTokenAuthenticator>()
                        .Append<TestBasicAuthenticator>(a => a.ActionMethod.Name.EndsWith("BasicSecure")))
                    .ConfigureBehaviors(b => b
                        .Append<AuthenticationBehavior>(a => a.ActionMethod.Name.EndsWith("Secure"))
                        .Append<Behavior2>()
                        .Append<Behavior2>()));

            configuration.EnsureInitialized();
        }
    }

    public class EmptyBehavior : BehaviorBase
    {
        public EmptyBehavior(IBehavior innerBehavior) : base(innerBehavior) { }
        public override Task<HttpResponseMessage> Invoke()
        {
            return InnerBehavior.Invoke();
        }
    }

    public class Behavior1 : EmptyBehavior
    {
        public Behavior1(IBehavior innerBehavior) : base(innerBehavior) { }
    }

    public class Behavior2 : EmptyBehavior
    {
        public Behavior2(IBehavior innerBehavior) : base(innerBehavior) { }
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