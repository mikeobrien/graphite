using System;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;

namespace Graphite.AspNet
{
    public static class Extensions
    {
        public static HttpConfiguration InitializeGraphite(
            this HttpConfiguration httpConfiguration,
            Action<ConfigurationDsl> configure = null)
        {
            var graphiteApplication = new GraphiteApplication(httpConfiguration);
            graphiteApplication.Initialize(x => Configure(x, configure), 
                Assembly.GetCallingAssembly());
            HostingEnvironment.StopListening += (s, e) => graphiteApplication.Dispose();
            return httpConfiguration;
        }

        private static void Configure(ConfigurationDsl configuration,
            Action<ConfigurationDsl> configure)
        {
            configure?.Invoke(configuration);

            configuration
                .WithPathProvider<AspNetPathProvider>()
                .WithRequestPropertyProvider<AspNetRequestPropertyProvider>();
        }
    }
}
