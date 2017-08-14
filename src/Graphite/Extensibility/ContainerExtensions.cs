using Graphite.DependencyInjection;
using Graphite.Extensions;
using Graphite.Linq;

namespace Graphite.Extensibility
{
    public static class ContainerExtensions
    {
        public static IContainer RegisterPlugins<TPlugin>(this IContainer container,
            Plugins<TPlugin> plugins)
            where TPlugin : class
        {
            plugins.ForEach(x => container.RegisterPlugin(x));
            return container;
        }

        public static IContainer RegisterPlugins<TPlugin, TContext>(this IContainer container,
            ConditionalPlugins<TPlugin, TContext> plugins) 
            where TPlugin : class
        {
            plugins.ForEach(x => container.RegisterPlugin(x));
            return container;
        }

        public static IContainer RegisterPlugin<TPlugin>(this IContainer container,
            Plugin<TPlugin> plugin) where TPlugin : class
        {
            if (plugin.HasInstance) container.Register(
                typeof(TPlugin), plugin.Instance, plugin.Dispose);
            else if (plugin.Type != null)
                container.Register<TPlugin>(plugin.Type, plugin.Singleton);
            return container;
        }

        public static Registry RegisterPlugins<TPlugin>(this Registry registry,
            Plugins<TPlugin> plugins)
            where TPlugin : class
        {
            plugins.ForEach(x => registry.RegisterPlugin(x));
            return registry;
        }

        public static Registry RegisterPlugins<TPlugin, TContext>(this Registry registry,
            ConditionalPlugins<TPlugin, TContext> plugins)
            where TPlugin : class
        {
            plugins.ForEach(x => registry.RegisterPlugin(x));
            return registry;
        }

        public static Registry RegisterPlugin<TPlugin>(this Registry registry,
            Plugin<TPlugin> plugin) where TPlugin : class
        {
            if (plugin.HasInstance) registry.Register(
                typeof(TPlugin), plugin.Instance, plugin.Dispose);
            else registry.Register<TPlugin>(plugin.Type, plugin.Singleton);
            return registry;
        }

        public static TPlugin GetInstance<TPlugin>(this Plugin<TPlugin> plugin,
            IContainer container) where TPlugin : class
        {
            return (TPlugin)(plugin.HasInstance ? plugin.Instance : 
                container.GetInstance(plugin.Type));
        }
    }
}
