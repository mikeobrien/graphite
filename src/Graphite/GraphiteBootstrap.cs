using System;
using System.Reflection;
using System.Web.Http;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
using Graphite.Monitoring;
using Graphite.Reflection;

namespace Graphite
{
    public static class GraphiteBootstrap
    {
        public static HttpConfiguration InitializeGraphite(
            this HttpConfiguration httpConfiguration,
            Action<ConfigurationDsl> configure = null)
        {
            Initialize(httpConfiguration, configure, Assembly.GetCallingAssembly());
            return httpConfiguration;
        }

        private static void Initialize(HttpConfiguration httpConfiguration, 
            Action<ConfigurationDsl> configure, Assembly defaultAssembly)
        {
            try
            {
                var metrics = new Metrics();
                metrics.BeginStartup();

                var configuration = new Configuration();

                if (!defaultAssembly.IsSystemAssembly())
                    configuration.Assemblies.Add(defaultAssembly);

                configure?.Invoke(new ConfigurationDsl(configuration));

                var container = new TrackingContainer(configuration.Container, new TypeCache());

                container.Register<IContainer>(container);
                container.Register(metrics);
                container.Register(configuration);
                container.Register(httpConfiguration);
                container.RegisterPlugins(configuration.ActionMethodSources);
                container.RegisterPlugins(configuration.ActionSources);
                container.RegisterPlugins(configuration.ActionDecorators);
                container.RegisterPlugins(configuration.RouteConventions);
                container.RegisterPlugin(configuration.InlineConstraintBuilder);
                container.RegisterPlugin(configuration.ConstraintBuilder);
                container.RegisterPlugins(configuration.UrlConventions);
                container.RegisterPlugin(configuration.TypeCache);
                container.RegisterPlugin(configuration.Initializer);
                container.RegisterPlugin(configuration.RouteMapper);
                container.RegisterPlugin(configuration.InlineConstraintResolver);
                container.RegisterPlugin(configuration.UnhandledExceptionHandler);
                container.RegisterPlugin(configuration.BehaviorChainInvoker);
                container.RegisterPlugins(configuration.Authenticators);
                container.RegisterPlugin(configuration.ActionInvoker);
                container.RegisterPlugins(configuration.RequestBinders);
                container.RegisterPlugins(configuration.RequestReaders);
                container.RegisterPlugins(configuration.ValueMappers);
                container.RegisterPlugins(configuration.ResponseWriters);
                container.IncludeRegistry(configuration.Registry);

                container.GetInstance<IInitializer>().Initialize();

                metrics.StartupComplete();
            }
            catch (Exception exception)
            {
                throw new GraphiteInitializationException(exception);
            }
        }
    }
}
