using Graphite;
using Graphite.Authentication;
using Graphite.Cors;
using Graphite.StructureMap;
using TestHarness.Handlers;
using TestHarness.Routing;

namespace TestHarness
{
    public static class Configuration
    {
        public static void Configure(ConfigurationDsl configure)
        {
            configure
                .ClearAssemblies()
                .IncludeThisAssembly()
                .EnableDiagnosticsInDebugMode()
                //.UseBender(x => x.UseCamelCaseNaming())
                //.ConfigureJsonNet(x => x.ContractResolver = 
                //    new CamelCasePropertyNamesContractResolver())
                .UseStructureMapContainer<Registry>()
                .ExcludeCurrentNamespaceFromUrl()
                .ConfigureActionDecorators(d => d.Append<TestActionDecorator>())
                .BindCookies()
                .BindRequestInfo()
                .BindHeaders()
                .BindContainer()
                .ReturnErrorMessages()
                .ConfigureHttpRouteDecorators(x => x
                    .Append<RoutingTestHandler.HttpRouteDecorator>(d => d
                        .ActionMethod.Name.Contains("Decorator")))
                .ConfigureAuthenticators(x => x
                    .Append<TestBearerTokenAuthenticator>()
                    .Append<TestBasicAuthenticator>(a => a.ActionMethod.Name.EndsWith("BasicSecure")))
                .ConfigureBehaviors(b => b
                    .Append<AuthenticationBehavior>(a => a.ActionMethod.Name.EndsWith("Secure"))
                    .Append<Behavior1>()
                    .Append<Behavior2>())
                .ConfigureCors(x => x
                    .AppendPolicy(p => p
                        .AllowOrigins("http://fark.com")
                        .AllowAnyMethod()
                        .AppliesWhen(a => a.ActionMethod.Name == "GetCorsPolicy1"))
                    .AppendPolicy(p => p
                        .AllowOrigins("http://farker.com")
                        .AllowAnyMethod()
                        .AppliesWhen(a => a.ActionMethod.Name == "GetCorsPolicy2")));
        }
    }
}
