using System;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensibility;
using Graphite.Readers;
using Graphite.Reflection;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;

        public ConfigurationDsl(Configuration configuration, HttpConfiguration httpConfiguration)
        {
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
        }
        
        /// <summary>
        /// Disables the built in metrics.
        /// </summary>
        public ConfigurationDsl DisableMetrics()
        {
            _configuration.Metrics = false;
            return this;
        }

        /// <summary>
        /// Specifies the initializer to use.
        /// </summary>
        public ConfigurationDsl WithInitializer<T>() where T : IInitializer
        {
            _configuration.Initializer.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the initializer to use.
        /// </summary>
        public ConfigurationDsl WithInitializer<T>(T instance) where T : IInitializer
        {
            _configuration.Initializer.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the type cache to use.
        /// </summary>
        public ConfigurationDsl WithTypeCache<T>() where T : ITypeCache
        {
            _configuration.TypeCache.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the type cache to use.
        /// </summary>
        public ConfigurationDsl WithTypeCache<T>(T instance) where T : ITypeCache
        {
            _configuration.TypeCache.Set(instance);
            return this;
        }

        /// <summary>
        /// Configures request readers.
        /// </summary>
        public ConfigurationDsl ConfigureRequestReaders(Action<ConditionalPluginsDsl
            <IRequestReader, ActionConfigurationContext>> configure)
        {
            _configuration.RequestReaders.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures value mappers.
        /// </summary>
        public ConfigurationDsl ConfigureValueMappers(Action<ConditionalPluginsDsl
            <IValueMapper, ValueMapperConfigurationContext>> configure)
        {
            _configuration.ValueMappers.Configure(configure);
            return this;
        }
    }
}
