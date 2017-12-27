using System.Collections;
using System.Linq;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Linq;
using Graphite.Monitoring;
using Graphite.Reflection;

namespace Graphite.Diagnostics
{
    public class ActionsSection : DiagnosticsSectionBase
    {
        private readonly Configuration _configuration;
        private readonly RuntimeConfiguration _runtimeConfiguration;
        private readonly Metrics _metrics;
        private readonly ITypeCache _typeCache;

        public ActionsSection(
            Configuration configuration, 
            RuntimeConfiguration runtimeConfiguration,
            Metrics metrics, ITypeCache typeCache) : 
            base("Actions")
        {
            _configuration = configuration;
            _runtimeConfiguration = runtimeConfiguration;
            _metrics = metrics;
            _typeCache = typeCache;
        }

        public override string Render()
        {
            var currentAssembly = _typeCache.GetTypeAssemblyDescriptor<ActionsSection>();
            return currentAssembly
                .GetResourceString<DiagnosticsHandler>("Actions.html")
                .RenderMustache(new
                {
                    assemblies = _configuration.Assemblies.Select(x => x.FullName.HtmlEncode()),
                    endpoints = _runtimeConfiguration.Actions.Select(x => new
                    {
                        method = x.Route.Method,
                        url = x.Route.Url.HtmlEncode(),
                        urlParts = x.Route.Url.HtmlEncode().Split("/")
                            .Select(s => new
                            {
                                segment = !s.StartsWith("{") ? s : null,
                                parameter = s.StartsWith("{") ? s : null
                            }),
                        actionName = x.Action.FullName,
                        assembly = x.Action.MethodDescriptor.DeclaringType.Type.Assembly.GetFriendlyName().HtmlEncode(),
                        averageRequestTime = _metrics.GetAverageRequestTime(x),
                        behaviors = x.Behaviors
                            .Select(b => b.Type.ToTypeDescriptor(_typeCache))
                            .Append(_configuration.DefaultBehavior.ToTypeDescriptor(_typeCache))
                            .ToListModel(b => b.FriendlyFullName.HtmlEncode()),
                        requestParameter = x.Route.RequestParameter.ToOptionalModel(p => new
                        {
                            name = p.Name,
                            type = p.ParameterType.FriendlyFullName.HtmlEncode()
                        }),
                        urlParameters  = x.Route.UrlParameters.ToListModel(p => new
                        {
                            name = p.Name,
                            type = p.TypeDescriptor.FriendlyFullName.HtmlEncode(),
                        }),
                        actionParameters = x.Route.Parameters.ToListModel(p => new
                        {
                            name = p.Name,
                            type = p.TypeDescriptor.FriendlyFullName.HtmlEncode(),
                        }),
                        responseType = x.Route.ResponseType.ToOptionalModel(t => 
                            t.FriendlyFullName.HtmlEncode()),
                        actionPlugins = new ArrayList
                        {
                            new
                            {
                                name = "Registry",
                                plugins = GetRegistry(x.Registry).ToListModel()
                            },
                            new
                            {
                                name = "Authenticators",
                                plugins = GetActionPlugins(x.Authenticators)
                            },
                            new
                            {
                                name = "Request Binders",
                                plugins = GetActionPlugins(x.RequestBinders)
                            },
                            new
                            {
                                name = "Request Readers",
                                plugins = GetActionPlugins(x.RequestReaders)
                            },
                            new
                            {
                                name = "Response Writers",
                                plugins = GetActionPlugins(x.ResponseWriters)
                            },
                            new
                            {
                                name = "Response Status",
                                plugins = GetActionPlugins(x.ResponseStatus)
                            },
                        }
                    })
                }, _typeCache,
                new
                {
                    actionPlugins = currentAssembly.GetResourceString<DiagnosticsHandler>("ActionPlugins.html")
                });
        }

        private IEnumerable GetRegistry(Registry registry)
        {
            return registry?.OrderBy(r => r.PluginType.FullName).Select(r =>
            {
                var pluginType = _typeCache.GetTypeDescriptor(r.PluginType);
                var concreteType = r.ConcreteType != null ? _typeCache.GetTypeDescriptor(r.ConcreteType) : null;
                return new
                {
                    type = pluginType.FriendlyFullName,
                    assembly = pluginType.Type.Assembly.GetFriendlyName().HtmlEncode(),
                    singleton = r.Singleton.ToYesNoModel(!r.IsInstance),
                    instance = r.IsInstance.ToYesNoModel(),
                    concreteType = concreteType?.FriendlyFullName,
                    concreteAssembly = concreteType?.Type.Assembly.GetFriendlyName().HtmlEncode()
                };
            });
        }

        private object GetActionPlugins<T>(Plugins<T> plugins)
        {
            return plugins.Select(p =>
            {
                var pluginType = _typeCache.GetTypeDescriptor(typeof(T));
                var concreteType = p.Type != null ? _typeCache.GetTypeDescriptor(p.Type) : null;
                return new
                {
                    type = pluginType.FriendlyFullName,
                    assembly = pluginType.Type.Assembly.GetFriendlyName().HtmlEncode(),
                    singleton = p.Singleton.ToYesNoModel(!p.HasInstance),
                    instance = p.HasInstance.ToYesNoModel(),
                    concreteType = concreteType?.FriendlyFullName,
                    concreteAssembly = concreteType?.Type.Assembly.GetFriendlyName().HtmlEncode()
                };
            }).ToListModel();
        }
    }
}