using Graphite.Hosting;
using Graphite.Http;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Specifies the path provider to use.
        /// </summary>
        public ConfigurationDsl WithPathProvider<T>() where T : IPathProvider
        {
            _configuration.PathProvider.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the path provider to use.
        /// </summary>
        public ConfigurationDsl WithPathProvider<T>(T instance) where T : IPathProvider
        {
            _configuration.PathProvider.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the request properties provider to use.
        /// </summary>
        public ConfigurationDsl WithRequestPropertyProvider<T>() 
            where T : IRequestPropertiesProvider
        {
            _configuration.RequestPropertiesProvider.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the request properties provider to use.
        /// </summary>
        public ConfigurationDsl WithRequestPropertyProvider<T>(T instance) 
            where T : IRequestPropertiesProvider
        {
            _configuration.RequestPropertiesProvider.Set(instance);
            return this;
        }
    }
}
