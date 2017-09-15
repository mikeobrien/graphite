using System;
using System.Threading;
using System.Web.Http;
using Graphite.Setup;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace Graphite.Owin
{
    public static class Extensions
    {
        public static IAppBuilder InitializeGraphite(
            this IAppBuilder builder, HttpServer server,
            Action<ConfigurationDsl> configure = null)
        {
            builder.UseWebApi(server);
            return builder.InitializeGraphite(server.Configuration,
                configure);
        }

        public static IAppBuilder InitializeGraphite(
            this IAppBuilder builder,
            Action<ConfigurationDsl> configure = null)
        {
            var httpConfiguration = new HttpConfiguration();
            builder.UseWebApi(httpConfiguration);
            return builder.InitializeGraphite(httpConfiguration, 
                configure);
        }

        public static IAppBuilder InitializeGraphite(
            this IAppBuilder builder, HttpConfiguration httpConfiguration,
            Action<ConfigurationDsl> configure = null)
        {
            var graphiteApplication = new GraphiteApplication(httpConfiguration);
            graphiteApplication.Initialize(x => Configure(x, configure));
            builder.OnAppDisposing(() => graphiteApplication.Dispose());
            return builder;
        }

        private static void Configure(ConfigurationDsl configuration,
            Action<ConfigurationDsl> configure)
        {
            configure?.Invoke(configuration);

            configuration
                .WithPathProvider<OwinPathProvider>()
                .WithRequestPropertyProvider<OwinRequestPropertyProvider>();
        }

        public static IAppBuilder OnAppDisposing(this IAppBuilder builder, Action disposing)
        {
            var token = new AppProperties(builder.Properties).OnAppDisposing;
            if (token != CancellationToken.None) token.Register(disposing);
            return builder;
        }
    }
}
