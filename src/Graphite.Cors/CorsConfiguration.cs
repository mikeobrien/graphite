using System.Web.Cors;
using Graphite.Actions;
using Graphite.Extensibility;

namespace Graphite.Cors
{
    public class CorsConfiguration
    {
        public Plugin<ICorsEngine> CorsEngine { get; } =
            Plugin<ICorsEngine>.Create<CorsEngine>(singleton: true);

        public ConditionalPlugins<ICorsPolicySource, ActionConfigurationContext> PolicySources { get; } =
            new ConditionalPlugins<ICorsPolicySource, ActionConfigurationContext>(false);
    }
}