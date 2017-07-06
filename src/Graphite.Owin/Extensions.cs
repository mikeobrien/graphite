using System;
using System.Reflection;
using System.Threading;
using System.Web.Http;
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
                configure, Assembly.GetCallingAssembly());
        }

        public static IAppBuilder InitializeGraphite(
            this IAppBuilder builder,
            Action<ConfigurationDsl> configure = null)
        {
            var httpConfiguration = new HttpConfiguration();
            builder.UseWebApi(httpConfiguration);
            return builder.InitializeGraphite(httpConfiguration, 
                configure, Assembly.GetCallingAssembly());
        }

        public static IAppBuilder InitializeGraphite(
            this IAppBuilder builder, HttpConfiguration httpConfiguration,
            Action<ConfigurationDsl> configure = null)
        {
            return builder.InitializeGraphite(httpConfiguration, 
                configure, Assembly.GetCallingAssembly());
        }

        private static IAppBuilder InitializeGraphite(
            this IAppBuilder builder, HttpConfiguration httpConfiguration,
            Action<ConfigurationDsl> configure, Assembly defaultAssembly)
        {
            var graphiteApplication = new GraphiteApplication(httpConfiguration);
            graphiteApplication.Initialize(x => Configure(x, configure), defaultAssembly);
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
