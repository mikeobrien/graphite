using System.Web.Cors;
using Graphite.Actions;
using Graphite.Extensibility;

namespace Graphite.Cors
{
    public class CorsConfiguration
    {
        public PluginDefinition<ICorsEngine> CorsEngine { get; } =
            PluginDefinition<ICorsEngine>.Create<CorsEngine>(singleton: true);

        public PluginDefinitions<ICorsPolicySource, ActionConfigurationContext> PolicySources { get; } =
            PluginDefinitions<ICorsPolicySource, ActionConfigurationContext>.Create();
    }
}