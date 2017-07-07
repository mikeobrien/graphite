using System;
using System.Linq;
using System.Web.Http;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
using Graphite.Monitoring;
using Graphite.Reflection;
using Graphite.Setup;
using Newtonsoft.Json;

namespace Graphite
{
    public class GraphiteApplication : IDisposable
    {
        private readonly HttpConfiguration _httpConfiguration;

        public GraphiteApplication(HttpConfiguration httpConfiguration)
        {
            _httpConfiguration = httpConfiguration;
        }

        public IContainer Container { get; private set; }
        public bool Initialized => Container != null;
        public Metrics Metrics { get; private set; }

        public void Initialize(Action<ConfigurationDsl> configure)
        {
            var configuration = new Configuration();

            configure?.Invoke(new ConfigurationDsl(configuration, _httpConfiguration));

            Initialize(configuration);
        }

        public void Initialize(Configuration configuration)
        {
            if (Initialized) return;

            if (!configuration.Assemblies.Any())
                throw new GraphiteException("No handler assemblies specified.");

            try
            {
                Metrics = new Metrics();
                Metrics.BeginStartup();
                
                configuration.Container.RegisterPlugin(configuration.TypeCache);

                Container = new TrackingContainer(configuration.Container, 
                    configuration.Container.GetInstance<ITypeCache>());

                Container.Register(Container);
                Container.Register(Metrics);
                Container.Register(configuration);
                Container.Register(_httpConfiguration);
                Container.RegisterPlugins(configuration.ActionMethodSources);
                Container.RegisterPlugins(configuration.ActionSources);
                Container.RegisterPlugins(configuration.ActionDecorators);
                Container.RegisterPlugins(configuration.RouteConventions);
                Container.RegisterPlugin(configuration.InlineConstraintBuilder);
                Container.RegisterPlugin(configuration.ConstraintBuilder);
                Container.RegisterPlugins(configuration.UrlConventions);
                Container.RegisterPlugin(configuration.Initializer);
                Container.RegisterPlugin(configuration.HttpRouteMapper);
                Container.RegisterPlugin(configuration.RequestPropertiesProvider);
                Container.RegisterPlugin(configuration.PathProvider);
                Container.RegisterPlugins(configuration.HttpRouteDecorators);
                Container.RegisterPlugin(configuration.InlineConstraintResolver);
                Container.RegisterPlugin(configuration.BehaviorChainInvoker);
                Container.RegisterPlugins(configuration.Authenticators);
                Container.RegisterPlugin(configuration.ActionInvoker);
                Container.Register(configuration.XmlWriterSettings);
                Container.Register(configuration.XmlReaderSettings);
                Container.Register(JsonSerializer.Create(
                    configuration.JsonSerializerSettings));
                Container.RegisterPlugins(configuration.RequestBinders);
                Container.RegisterPlugins(configuration.RequestReaders);
                Container.RegisterPlugins(configuration.ValueMappers);
                Container.RegisterPlugins(configuration.ResponseWriters);
                Container.RegisterPlugins(configuration.ResponseStatus);
                Container.IncludeRegistry(configuration.Registry);

                Container.GetInstance<IInitializer>().Initialize();

                Metrics.StartupComplete();
            }
            catch (Exception exception)
            {
                throw new GraphiteInitializationException(exception);
            }
        }

        public void Dispose()
        {
            Container?.Dispose();
        }
    }
}
