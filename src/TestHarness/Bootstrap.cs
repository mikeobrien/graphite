﻿using System;
using System.Web.Http;
using Graphite.Authentication;
using Graphite.Cors;
using Graphite.Http;
using Graphite.Setup;
using Graphite.StructureMap;
using Graphite.Views;
using TestHarness.Handlers;
using TestHarness.Routing;

namespace TestHarness
{
    public static class Bootstrap
    {
        public static void Configure(ConfigurationDsl configure)
        {
            configure
                .ClearAssemblies()
                .IncludeThisAssembly()
                .EnableDiagnosticsInDebugMode()
                .ConfigureWebApi(x => x
                    .RouteExistingFiles()
                    .SetExceptionHandler<DebugExceptionHandler>())
                //.ConfigureSerialization(s => s
                //    .Json(j => j.UseCamelCaseNaming())
                //    .Xml(x => x
                //        .Reader(r => r.IgnoreComments = true)
                //        .Writer(w => w.Indent = true)))
                .UseStructureMapContainer<Registry>()
                .ConfigureActionDecorators(d => d.Append<TestActionDecorator>())
                .BindCookies()
                .BindRequestInfo()
                .BindHeaders()
                .BindContainer()
                .EnableViews()
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
                .EnableCors(x => x
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
