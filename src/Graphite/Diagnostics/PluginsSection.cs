using System;
using System.Collections;
using System.Linq;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Diagnostics
{
    public class PluginsSection : DiagnosticsSectionBase
    {
        private readonly Configuration _configuration;
        private readonly ITypeCache _typeCache;

        public PluginsSection(
            Configuration configuration, 
            ITypeCache typeCache) : 
            base("Plugins")
        {
            _configuration = configuration;
            _typeCache = typeCache;
        }

        public override string Render()
        {
            return _typeCache.GetTypeAssemblyDescriptor<PluginsSection>()
                .GetResourceString<DiagnosticsHandler>("Plugins.html")
                .RenderMustache(new
                {
                    plugins = new ArrayList
                    {
                        GetPlugins("Type Cache", _configuration.TypeCache),
                        GetPlugins("Path Provider", _configuration.PathProvider),
                        GetPlugins("Diagnostics Provider", _configuration.DiagnosticsProvider),
                        GetPlugins("Diagnostics Sections", _configuration.DiagnosticsSections),

                        GetPlugins("Initializer", _configuration.Initializer),
                        GetPlugins("Action Method Source", _configuration.ActionMethodSources),
                        GetPlugins("Action Source", _configuration.ActionSources),
                        GetPlugins("Route Convention", _configuration.RouteConventions),
                        GetPlugins("Route Mapper", _configuration.HttpRouteMapper),
                        GetPlugins("Url Convention", _configuration.UrlConventions),
                        GetPlugins("Constraint Builder", _configuration.ConstraintBuilder),
                        GetPlugins("Inline Constraint Resolver", _configuration.InlineConstraintResolver),
                        GetPlugins("inline Constraint Builder", _configuration.InlineConstraintBuilder),
                        GetPlugins("Action Decorator", _configuration.ActionDecorators),
                
                        GetPlugins("Behavior Chain Invoker", _configuration.BehaviorChainInvoker),
                        GetPlugins("Action Invoker", _configuration.ActionInvoker),
                        GetPlugins("Value Mapper", _configuration.ValueMappers),
                        GetPlugins("Request Properties", _configuration.RequestPropertiesProvider),
                        GetPlugins("Url Parameters", _configuration.UrlParameters),
                        GetPlugins("Querystring", _configuration.QuerystringParameters),

                        GetPlugins("Behavior", _configuration.Behaviors),
                        GetPlugins("Authenticator", _configuration.Authenticators),
                        GetPlugins("Request Binder", _configuration.RequestBinders),
                        GetPlugins("Request Reader", _configuration.RequestReaders),
                        GetPlugins("Response Status", _configuration.ResponseStatus),
                        GetPlugins("Response Headers", _configuration.ResponseHeaders),
                        GetPlugins("Response Writer", _configuration.ResponseWriters)
                    }
                }, _typeCache);
        }
        
        private object GetPlugins<T>(string name, Plugin<T> plugin)
        {
            return new ArrayList
            {
                GetPlugin(name, plugin.Type)
            }.ToListModel();
        }

        private object GetPlugins<TPlugin>(string name, Plugins<TPlugin> plugins)
        {
            return plugins.Select(x => GetPlugin(name, x.Type, false, 
                plugins.IndexOf((Plugin<TPlugin>) x), x.Singleton)).ToListModel(name);
        }

        private object GetPlugins<TPlugin, TContext>(string name, 
            ConditionalPlugins<TPlugin, TContext> plugins)
        {
            return plugins.Select(x => GetPlugin(name, x.Type, x.AppliesTo != null,
                plugins.IndexOf(x), x.Singleton)).ToListModel(name);
        }

        private object GetPlugin(string name, Type type,
            bool? appliesTo = null, int? order = null,
            bool? singleton = null)
        {
            return new
            {
                name = name,
                order = order.ToOptionalModel(x => x + 1),
                singleton = singleton.ToYesNoModel(),
                appliesTo = appliesTo.ToYesNoModel(),
                type = type?.GetFriendlyTypeName(true).HtmlEncode(),
                assembly = type?.Assembly.GetFriendlyName().HtmlEncode()
            };
        }
    }
}