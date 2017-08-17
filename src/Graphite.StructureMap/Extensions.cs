using System;
using System.Linq;
using Graphite.DependencyInjection;
using Graphite.Setup;
using StructureMap;
using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Pipeline;

namespace Graphite.StructureMap
{
    public static class Extensions
    {
        public static ConfigurationDsl UseStructureMapContainer<TRegistry>(
            this ConfigurationDsl configuration)
            where TRegistry : global::StructureMap.Registry, new()
        {
            return configuration.UseStructureMapContainer(x => x.AddRegistry<TRegistry>());
        }

        public static ConfigurationDsl UseStructureMapContainer(
            this ConfigurationDsl configuration,
            Action<ConfigurationExpression> configure = null,
            bool isDependencyResolver = true)
        {
            var container = configure == null ? new Container() : new Container(configure);
            if (isDependencyResolver)
                configuration.ConfigureWebApi(x => x.Configuration
                    .DependencyResolver = container);
            return configuration.UseContainer(container);
        }

        public static ConfigurationDsl UseStructureMapContainer(this 
            ConfigurationDsl configuration, global::StructureMap.IContainer container, 
            bool isDependencyResolver = true)
        {
            var graphiteContainer = new Container(container);
            if (isDependencyResolver)
                configuration.ConfigureWebApi(x => x.Configuration
                    .DependencyResolver = graphiteContainer);
            return configuration.UseContainer(graphiteContainer);
        }

        public static ExplicitArguments ToExplicitArgs(this Dependency[] dependencies)
        {
            var arguments = new ExplicitArguments();
            dependencies.ToList().ForEach(x => arguments.Set(x.Type, x.Instance));
            return arguments;
        }

        public static GenericFamilyExpression WithNoLifecycle(this GenericFamilyExpression expression)
        {
            expression.LifecycleIs(new NoLifecycle());
            return expression;
        }

        public static GenericFamilyExpression UseLightweightInstance(
            this GenericFamilyExpression expression, object instance)
        {
            expression.Use(new LightweightObjectInstance(instance));
            return expression;
        }

        public static GenericFamilyExpression Use(this GenericFamilyExpression expression,
            object instance, bool dispose)
        {
            if (dispose) expression.Use(instance);
            else expression.UseLightweightInstance(instance).WithNoLifecycle();
            return expression;
        }
    }
}
