using Graphite;
using Graphite.Authentication;
using Graphite.Cors;
using Graphite.Extensions;
using Graphite.StructureMap;
using Newtonsoft.Json.Serialization;
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
                //.ConfigureSerialization(s => s
                //    .Json(j => j.UseCamelCaseNaming())
                //    .Xml(x => x
                //        .Reader(r => r.IgnoreComments = true)
                //        .Writer(w => w.Indent = true)))
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
