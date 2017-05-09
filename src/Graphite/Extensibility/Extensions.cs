using System.Collections.Generic;
using System.Linq;
using Graphite.DependencyInjection;
using Graphite.Extensions;

namespace Graphite.Extensibility
{
    public static class Extensions
    {
        public static void RegisterPlugins<TPlugin, TContext>(this IContainer container,
            PluginDefinitions<TPlugin, TContext> pluginDefinitions) 
            where TPlugin : class, IConditional<TContext>
        {
            pluginDefinitions.ForEach(pd => container.Register<TPlugin>(pd.Type, pd.Singleton));
        }

        public static void RegisterPlugin<TPlugin>(this IContainer container,
            PluginDefinition<TPlugin> pluginDefinition) where TPlugin : class
        {
            if (pluginDefinition.HasInstance) container.Register(pluginDefinition.Instance);
            else container.Register<TPlugin>(pluginDefinition.Type, pluginDefinition.Singleton);
        }

        public static IEnumerable<TPlugin> ThatApplyTo<TPlugin, TContext>(
            this PluginDefinitions<TPlugin, TContext> definitions,
            IEnumerable<TPlugin> plugins, TContext context)
            where TPlugin : IConditional<TContext>
        {
            return plugins
                .Where(x => x != null)
                .GroupJoin(definitions, d => d.GetType(), p => p.Type, (p, d) => new
                {
                    Instance = p,
                    Order = definitions.Order(p),
                    d.FirstOrDefault()?.AppliesTo
                })
                .Where(x => (x.AppliesTo?.Invoke(context) ?? true) && x.Instance.AppliesTo(context))
                .OrderBy(x => x.Order)
                .Select(x => x.Instance).ToList();
        }
    }
}
