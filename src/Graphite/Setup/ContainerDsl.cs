using System;
using Graphite.DependencyInjection;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Specifies the IoC container to use.
        /// </summary>
        public ConfigurationDsl UseContainer(IContainer container)
        {
            _configuration.Container = container;
            return this;
        }

        /// <summary>
        /// Specifies the IoC container to use.
        /// </summary>
        public ConfigurationDsl UseContainer<T>() where T : IContainer, new()
        {
            _configuration.Container = new T();
            return this;
        }

        /// <summary>
        /// Configures the IoC container.
        /// </summary>
        public ConfigurationDsl ConfigureRegistry(Action<Registry> configure)
        {
            configure(_configuration.Registry);
            return this;
        }
    }
}
