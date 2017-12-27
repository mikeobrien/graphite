using System;
using System.Reflection;
using Graphite.Diagnostics;
using Graphite.Extensibility;
using Graphite.Reflection;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Enables the diagnostics page.
        /// </summary>
        public ConfigurationDsl EnableDiagnostics()
        {
            _configuration.Diagnostics = true;
            return this;
        }

        /// <summary>
        /// Enables the diagnostics page when the
        /// calling assembly is in debug mode.
        /// </summary>
        public ConfigurationDsl EnableDiagnosticsInDebugMode()
        {
            _configuration.Diagnostics = Assembly
                .GetCallingAssembly().IsInDebugMode();
            return this;
        }

        /// <summary>
        /// Enables the diagnostics page when the 
        /// type assembly is in debug mode.
        /// </summary>
        public ConfigurationDsl EnableDiagnosticsInDebugMode<T>()
        {
            _configuration.Diagnostics = typeof(T).Assembly.IsInDebugMode();
            return this;
        }

        /// <summary>
        /// Sets the url of the diagnostics page.
        /// </summary>
        public ConfigurationDsl WithDiagnosticsAtUrl(string url)
        {
            _configuration.DiagnosticsUrl = url;
            return this;
        }

        /// <summary>
        /// Excludes the diagnostics pages from authentication.
        /// </summary>
        public ConfigurationDsl ExcludeDiagnosticsFromAuthentication()
        {
            _configuration.ExcludeDiagnosticsFromAuthentication = true;
            return this;
        }

        /// <summary>
        /// Set the diagnostics provider.
        /// </summary>
        public ConfigurationDsl WithDiagnosticsProvider<T>(bool singleton = false) 
            where T : IDiagnosticsProvider
        {
            _configuration.DiagnosticsProvider.Set<T>(singleton);
            return this;
        }

        /// <summary>
        /// Configures diagnostic page sections.
        /// </summary>
        public ConfigurationDsl ConfigureDiagnosticsSections(
            Action<PluginsDsl<IDiagnosticsSection>> configure)
        {
            _configuration.DiagnosticsSections.Configure(configure);
            return this;
        }
    }
}
