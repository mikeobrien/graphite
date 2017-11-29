using System.Linq;
using Graphite.Linq;
using Graphite.Routing;
using Swank.Configuration;
using Swank.Description;

namespace Graphite.Swank
{
    public static class Extensions
    {
        public static ConfigurationDsl UseGraphite(this ConfigurationDsl configurationDsl)
        {
            configurationDsl.WithApiExplorer<GraphiteApiExplorer>();
            return configurationDsl;
        }

        public static bool IsAlias(this IApiDescription description, string url)
        {
            return description.GetActionAttributes<UrlAliasAttribute>()
                .Any(a => a.Urls.ContainsUncase(url));
        }
    }
}
