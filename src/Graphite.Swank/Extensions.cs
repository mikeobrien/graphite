using Swank.Configuration;

namespace Graphite.Swank
{
    public static class Extensions
    {
        public static ConfigurationDsl UseGraphite(this ConfigurationDsl configurationDsl)
        {
            configurationDsl.WithApiExplorer<GraphiteApiExplorer>();
            return configurationDsl;
        }
    }
}
