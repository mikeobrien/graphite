using System;
using System.Collections.Generic;
using System.Linq;
using Graphite;
using Graphite.Actions;
using Graphite.Diagnostics;
using Graphite.Extensions;
using Graphite.Monitoring;
using Graphite.Reflection;
using Graphite.Routing;
using Graphite.StructureMap;
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
            configuration.Behaviors.Append<TestBehavior1>();
            configuration.Behaviors.Append<TestBehavior2>();
            var parameters = new TypeCache().GetTypeDescriptor(typeof(Handler)).Methods
                .FirstOrDefault(x => x.Name == nameof(Handler.Get)).Parameters;
            var actionDescriptors = new List<ActionDescriptor>
            {
                new ActionDescriptor(
                    ActionMethod.From<Handler>(x => x.Get(null, 0, DateTime.MaxValue)), 
                    new RouteDescriptor("GET", "some/url", 
                        parameters.Where(x => x.Name == "urlParam")
                            .Select(x => new UrlParameter(x, false)).ToArray(), 
                        parameters.Where(x => x.Name == "queryParam")
                            .Select(x => new ActionParameter(x)).ToArray(), 
                        parameters.First(x => x.Name == "request"),
                        new TypeCache().GetTypeDescriptor(typeof(OutputModel))),
                    new TypeDescriptor[] {})
            };
            var runtimeConfiguration = new RuntimeConfiguration(actionDescriptors);
            var handler = new DiagnosticsHandler(configuration, runtimeConfiguration, 
                new Metrics(), new Container(), new TypeCache());

            var result = handler.Get();

            result.ShouldContain(configuration.UnhandledExceptionStatusText);
            result.ShouldContain(configuration.HandlerNameFilterRegex);
            result.ShouldContain(configuration.HandlerNamespaceRegex);

            result.ShouldContain(typeof(GraphiteBootstrap).Assembly.GetFriendlyName());
            result.ShouldContain(typeof(GraphiteBootstrap).Assembly.GetName().Version.ToString());

            configuration.SupportedHttpMethods.ForEach(x => result.ShouldContain(x.Method));

            result.ShouldContain(configuration.Initializer.Type.GetFriendlyTypeName());
            result.ShouldContain(configuration.TypeCache.Type.GetFriendlyTypeName());
            result.ShouldContain(configuration.BehaviorChainInvoker.Type.GetFriendlyTypeName());
            result.ShouldContain(configuration.DefaultBehavior.Type.GetFriendlyTypeName());

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
