using System;
using System.Linq;
using System.Web.Http;
using Graphite.DependencyInjection;
using StructureMap;
using StructureMap.Pipeline;

namespace Graphite.StructureMap
{
    public static class Extensions
    {
        public static ConfigurationDsl UseStructureMapContainer<TRegistry>(
            this ConfigurationDsl configuration,
            HttpConfiguration httpConfiguration) 
            where TRegistry : global::StructureMap.Registry, new()
        {
            return configuration.UseStructureMapContainer(httpConfiguration,
                x => x.AddRegistry<TRegistry>());
        }

        public static ConfigurationDsl UseStructureMapContainer(
            this ConfigurationDsl configuration,
            HttpConfiguration httpConfiguration,
            Action<ConfigurationExpression> configure = null)
        {
            var container = configure == null ? new Container() : new Container(configure);
            httpConfiguration.DependencyResolver = container;
            return configuration.UseContainer(container);
        }

        public static ConfigurationDsl UseStructureMapContainer<TRegistry>(
            this ConfigurationDsl configuration)
            where TRegistry : global::StructureMap.Registry, new()
        {
            return configuration.UseStructureMapContainer(x => x.AddRegistry<TRegistry>());
        }

        public static ConfigurationDsl UseStructureMapContainer(
            this ConfigurationDsl configuration,
            Action<ConfigurationExpression> configure = null)
        {
            return configuration.UseContainer(configure == null ? 
                new Container() : new Container(configure));
        }

        public static ConfigurationDsl UseStructureMapContainer(this 
            ConfigurationDsl configuration, global::StructureMap.IContainer container)
        {
            return configuration.UseContainer(new Container(container));
        }

        public static ExplicitArguments ToExplicitArgs(this Dependency[] dependencies)
        {
            var arguments = new ExplicitArguments();
            dependencies.ToList().ForEach(x => arguments.Set(x.Type, x.Instance));
            return arguments;
        }
    }
}
