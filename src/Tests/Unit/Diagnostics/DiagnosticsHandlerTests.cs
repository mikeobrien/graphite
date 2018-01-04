﻿using System;
using System.Collections.Generic;
using System.Linq;
using Graphite;
using Graphite.Actions;
using Graphite.Diagnostics;
using Graphite.Linq;
using Graphite.Monitoring;
using Graphite.Reflection;
using Graphite.Routing;
using Graphite.StructureMap;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using Tests.Common.Fakes;

namespace Tests.Unit.Diagnostics
{
    [TestFixture]
    public class DiagnosticsHandlerTests
    {
        public class InputModel { }
        public class OutputModel { }

        public class Handler
        {
            public OutputModel Get(InputModel request, int queryParam, DateTime urlParam)
            {
                return null;
            }
        }

        [Test]
        public void Should_return_configuration()
        {
            var configuration = new Configuration();
            configuration.Behaviors.Configure(c => c
                .Append<TestBehavior1>()
                .Append<TestBehavior2>());
            var parameters = new TypeCache().GetTypeDescriptor(typeof(Handler)).Methods
                .FirstOrDefault(x => x.Name == nameof(Handler.Get))?.Parameters;
            var actionMethod = ActionMethod.From<Handler>(x => x.Get(null, 0, DateTime.MaxValue));
            var routeDescriptor = new RouteDescriptor("GET", "some/url", 
                parameters.Where(x => x.Name == "urlParam")
                    .Select(x => new UrlParameter(actionMethod, x, false)).ToArray(), 
                parameters.Where(x => x.Name == "queryParam")
                    .Select(x => new ActionParameter(actionMethod, x)).ToArray(), 
                parameters.First(x => x.Name == "request"),
                new TypeCache().GetTypeDescriptor(typeof(OutputModel)));
            var actionDescriptors = new List<ActionDescriptor>
            {
                new ActionDescriptorFactory(configuration, null, new TypeCache())
                    .CreateDescriptor(actionMethod ,routeDescriptor)
            };
            var metrics = new Metrics();
            var typeCache = new TypeCache();
            var runtimeConfiguration = new RuntimeConfiguration(actionDescriptors);
            var handler = new DiagnosticsHandler(new DiagnosticsProvider(configuration, 
                typeCache, new List<IDiagnosticsSection>
                {
                    new ConfigurationSection(configuration, new JsonSerializerSettings(), metrics, typeCache),
                    new ActionsSection(configuration, runtimeConfiguration, metrics, typeCache),
                    new PluginsSection(configuration, typeCache),
                    new ContainersSection(new Container(), typeCache)
                }), typeCache);

            var result = handler.Get();

            //result.ShouldContain(DefaultActionMethodSource.DefaultHandlerNameConventionRegex);
            //result.ShouldContain(DefaultRouteConvention.DefaultHandlerNamespaceConventionRegex);

            result.ShouldContain(typeof(GraphiteApplication).Assembly.GetFriendlyName());
            result.ShouldContain(typeof(GraphiteApplication).Assembly.GetName().Version.ToString());

            configuration.SupportedHttpMethods.ForEach(x => result.ShouldContain(x.Method));

            result.ShouldContain(configuration.Initializer.Type.GetFriendlyTypeName());
            result.ShouldContain(configuration.TypeCache.Type.GetFriendlyTypeName());
            result.ShouldContain(configuration.BehaviorChainInvoker.Type.GetFriendlyTypeName());
            result.ShouldContain(configuration.DefaultBehavior.GetFriendlyTypeName());

            configuration.ActionMethodSources.ForEach(x => result.ShouldContain(x.Type.GetFriendlyTypeName()));
            configuration.ActionSources.ForEach(x => result.ShouldContain(x.Type.GetFriendlyTypeName()));
            configuration.RouteConventions.ForEach(x => result.ShouldContain(x.Type.GetFriendlyTypeName()));
            configuration.UrlConventions.ForEach(x => result.ShouldContain(x.Type.GetFriendlyTypeName()));
            configuration.RequestReaders.ForEach(x => result.ShouldContain(x.Type.GetFriendlyTypeName()));
            configuration.RequestBinders.ForEach(x => result.ShouldContain(x.Type.GetFriendlyTypeName()));
            configuration.ValueMappers.ForEach(x => result.ShouldContain(x.Type.GetFriendlyTypeName()));
            configuration.ResponseWriters.ForEach(x => result.ShouldContain(x.Type.GetFriendlyTypeName()));
            configuration.Behaviors.ForEach(x => result.ShouldContain(x.Type.GetFriendlyTypeName()));

            result.ShouldContain(typeof(TestBehavior1).GetFriendlyTypeName());
            result.ShouldContain(typeof(TestBehavior2).GetFriendlyTypeName());
            result.ShouldContain("some/url");
            result.ShouldContain("request");
            result.ShouldContain(typeof(InputModel).GetFriendlyTypeName());
            result.ShouldContain("queryParam");
            result.ShouldContain(typeof(int).GetFriendlyTypeName());
            result.ShouldContain("urlParam");
            result.ShouldContain(typeof(DateTime).GetFriendlyTypeName());
            result.ShouldContain(typeof(OutputModel).GetFriendlyTypeName());
        }
    }
}
