using System;
using System.Collections;
using System.IO;
using Graphite.Extensions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
using Graphite.Http;
using Graphite.Linq;
using Graphite.Monitoring;
using Graphite.Reflection;
using Graphite.Writers;

namespace Graphite.Diagnostics
{
    public class DiagnosticsHandler
    {
        private static readonly Configuration DefaultConfiguration = new Configuration();
        private readonly RuntimeConfiguration _runtimeConfiguration;
        private readonly Configuration _configuration;
        private readonly Metrics _metrics;
        private readonly ITypeCache _typeCache;
        private readonly TrackingContainer _container;
        private readonly AssemblyDescriptor _assembly;
        
        const string NotApplicable = "<span class=\"grey\">N/A</span>";

        public DiagnosticsHandler(Configuration configuration, 
            RuntimeConfiguration runtimeConfiguration,
            Metrics metrics, IContainer container,
            ITypeCache typeCache)
        {
            _runtimeConfiguration = runtimeConfiguration;
            _configuration = configuration;
            _metrics = metrics;
            _typeCache = typeCache;
            _container = container as TrackingContainer;
            _assembly = typeCache.GetCurrentAssemblyDescriptor();
        }

        public string Get()
        {
            return _assembly.GetResourceString<DiagnosticsHandler>("Diagnostics.html")
                .RenderMustache(new
                {
                    url = $"/{_configuration.DiagnosticsUrl.Trim('/')}",
                    styles = _assembly.GetResourceString<DiagnosticsHandler>("Diagnostics.css"),
                    scripts = _assembly.GetResourceString<DiagnosticsHandler>("Diagnostics.js"),
                    version = Assembly.GetExecutingAssembly().GetName().Version,
                    configuration = GetConfiguration(),
                    plugins = GetPlugins(),
                    containers = GetContainers(),
                    actions = GetActions()
                }, _typeCache,
                new
                {
                    configuration = _assembly.GetResourceString<DiagnosticsHandler>("Configuration.html"),
                    actions = _assembly.GetResourceString<DiagnosticsHandler>("Actions.html"),
                    containers = _assembly.GetResourceString<DiagnosticsHandler>("Containers.html"),
                    plugins = _assembly.GetResourceString<DiagnosticsHandler>("Plugins.html")
                });
        }

        [OutputStream(MimeTypes.ImagePng)]
        public Stream GetFavicon()
        {
            return _assembly.GetResourceStream<DiagnosticsHandler>("favicon.png");
        }

        [OutputStream(MimeTypes.ImagePng)]
        public Stream GetLogo()
        {
            return _assembly.GetResourceStream<DiagnosticsHandler>("logo.png");
        }

        private object GetConfiguration()
        {
            return new
            {
                startupTime = _metrics.StartupTime,
                totalRequests = _metrics.TotalRequests,
                handlerNameConvention = _configuration.HandlerNameConvention,
                urlAliases = HowManyConfigured(_configuration.UrlAliases),
                handlerFilter = SetNotSetDefault(_configuration.HandlerFilter, DefaultConfiguration.HandlerFilter),
                actionRegex = SetNotSetDefault(_configuration.ActionNameConvention, DefaultConfiguration.ActionNameConvention),
                actionFilter = SetNotSetDefault(_configuration.ActionFilter, DefaultConfiguration.ActionFilter),
                actionMethodName = SetNotSetDefault(_configuration.ActionSegmentsConvention, DefaultConfiguration.ActionSegmentsConvention),
                httpMethod = SetNotSetDefault(_configuration.HttpMethodConvention, DefaultConfiguration.HttpMethodConvention),
                httpMethods = _configuration.SupportedHttpMethods.Select(x => new
                {
                    method = x.Method,
                    allowRequestBody = YesNo(x.AllowRequestBody),
                    allowResponseBody = YesNo(x.AllowResponseBody)
                })
            };
        }

        private string SetNotSetDefault(object value, object @default)
        {
            return value == @default
                ? "<span class=\"grey\">Default</span>"
                : value != null
                    ? "<span class=\"green\">Custom</span>"
                    : "<span class=\"red\">Not set</span>";
        }

        private string HowManyConfigured(IEnumerable value)
        {
            var items = value.Cast<object>().ToList();
            return items.Any()
                ? $"<span class=\"green\">{items.Count} configured</span>"
                : "<span class=\"red\">None configured</span>";
        }

        private object GetContainers()
        {
            return new ArrayList
            {
                new
                {
                    name = "Default Container",
                    registrations = GetRegistry(_container?.Parent?.As<TrackingContainer>()?.Registry)
                },
                new
                {
                    name = "Request Container",
                    registrations = GetRegistry(_container?.Registry)
                }
            };
        }

        private object GetRegistry(Registry registry)
        {
            return registry?.OrderBy(r => r.PluginType.FullName).Select(r =>
            {
                var pluginType = _typeCache.GetTypeDescriptor(r.PluginType);
                var concreteType = r.ConcreteType != null ? _typeCache.GetTypeDescriptor(r.ConcreteType) : null;
                return new
                {
                    type = pluginType.FriendlyFullName,
                    assembly = pluginType.Type.Assembly.GetFriendlyName().HtmlEncode(),
                    singleton = r.IsInstance ? NotApplicable : YesNo(r.Singleton),
                    instance = YesNo(r.IsInstance),
                    concreteType = concreteType?.FriendlyFullName,
                    concreteAssembly = concreteType?.Type.Assembly.GetFriendlyName().HtmlEncode()
                };
            });
        }

        private static readonly Regex UrlRegex = new Regex(@"(\{\w*\})");

        private object GetActions()
        {
            return new
            {
                assemblies = _configuration.Assemblies.Select(x => x.FullName.HtmlEncode()),
                endpoints = _runtimeConfiguration.Actions.Select(x => new
                {
                    method = x.Route.Method,
                    methodClass = GetMethodClass(x.Route.Method),
                    url = UrlRegex.Replace(x.Route.Url.HtmlEncode(), "<span class=\"url-parameter\">$1</span>"),
                    actionName = x.Action.FullName,
                    assembly = x.Action.MethodDescriptor.DeclaringType.Type.Assembly.GetFriendlyName().HtmlEncode(),
                    averageRequestTime = _metrics.GetAverageRequestTime(x),
                    behaviors = x.Behaviors
                        .Select(b => b.Type.ToTypeDescriptor(_typeCache))
                        .Append(_configuration.DefaultBehavior.ToTypeDescriptor(_typeCache))
                        .Select(b => $@"<tr><td><code>{b.FriendlyFullName.HtmlEncode()}</code></td></tr>")
                        .Join("<tr><td class=\"arrow\">&darr;</td></tr>"),
                    requestParameter = x.Route.RequestParameter == null 
                        ? "<span class=\"red\">None</span>" 
                        : $@"<code class=""red"">{x.Route.RequestParameter.Name}</code>:<code>{x.Route
                            .RequestParameter.ParameterType.FriendlyFullName.HtmlEncode()}</code>",
                    urlParameters = !x.Route.UrlParameters.Any() 
                        ? "<span class=\"red\">None</span>" 
                        : x.Route.UrlParameters.Select(p => $@"<code class=""red"">{p.Name}</code>:<code>
                            {p.TypeDescriptor.FriendlyFullName.HtmlEncode()}</code>").Join(", "),
                    parameters = !x.Route.Parameters.Any() 
                        ? "<span class=\"red\">None</span>" 
                        : x.Route.Parameters.Select(p => $@"<code class=""red"">{p.Name}</code>:<code>
                            {p.TypeDescriptor.FriendlyFullName.HtmlEncode()}</code>").Join(", "),
                    response = x.Route.ResponseType == null 
                        ? "<span class=\"red\">None</span>" 
                        : $"<code>{x.Route.ResponseType.FriendlyFullName.HtmlEncode()}</code>",
                    registry = new
                    {
                        empty = !x.Registry.Any(),
                        notEmpty = x.Registry.Any(),
                        registrations = GetRegistry(x.Registry)
                    }
                })
            };
        }
        
        private string GetMethodClass(string method)
        {
            switch (method)
            {
                case "GET": return "dark-blue";
                case "POST": return "green";
                case "PUT": return "pink";
                case "PATCH": return "purple";
                case "DELETE": return "red";
                default: return "";
            }
        }
        
        private object GetPlugins()
        {
            return new
            {
                types = new ArrayList
                {
                    GetPlugins("Initializer", _configuration.Initializer),
                    GetPlugins("Type Cache", _configuration.TypeCache),
                    GetPlugins("Action Method Source", _configuration.ActionMethodSources),
                    GetPlugins("Action Source", _configuration.ActionSources),
                    GetPlugins("Route Convention", _configuration.RouteConventions),
                    GetPlugins("Url Convention", _configuration.UrlConventions),
                    GetPlugins("Behavior Chain Invoker", _configuration.BehaviorChainInvoker),
                    GetPlugins("Behavior", _configuration.Behaviors),
                    GetPlugins("Action Invoker", _configuration.ActionInvoker),

                    GetPlugins("Authenticators", _configuration.Authenticators),
                    GetPlugins("Request Binder", _configuration.RequestBinders),
                    GetPlugins("Request Reader", _configuration.RequestReaders),
                    GetPlugins("Parameter Mapper", _configuration.ValueMappers),
                    GetPlugins("Response Writer", _configuration.ResponseWriters)
                }
            };
        }

        private object GetPlugins<T>(string name, Plugin<T> plugin)
        {
            return new
            {
                registrations = new ArrayList
                {
                    GetPlugin(name, plugin.Type)
                }
            };
        }

        private object GetPlugins<TPlugin>(string name, Plugins<TPlugin> plugins)
        {
            return GetPlugins(name, plugins, () => plugins
                .Select(x => GetPlugin(name, x.Type,
                    false, plugins.IndexOf(x), x.Singleton)));
        }

        private object GetPlugins<TPlugin, TContext>(string name, 
            ConditionalPlugins<TPlugin, TContext> plugins)
        {
            return GetPlugins(name, plugins, () => plugins
                .Select(x => GetPlugin(name, x.Type,
                    x.AppliesTo != null, plugins.IndexOf(x), x.Singleton)));
        }

        private object GetPlugins(string name, IEnumerable plugins, 
            Func<IEnumerable> results)
        {
            var hasPlugins = plugins.Cast<object>().Any();
            return new
            {
                registrations = hasPlugins 
                    ? results() 
                    : null,
                noneConfigured = !hasPlugins 
                    ? new 
                        {
                            name = name
                        } 
                    : null
            };
        }

        private object GetPlugin(string name, Type type,
            bool? appliesTo = null, int? order = null,
            bool? singleton = null)
        {
            return new
            {
                name = name,
                order = order.HasValue ? $"<code>{order + 1}</code>" : NotApplicable,
                singleton = singleton.HasValue ? YesNo(singleton.Value) : NotApplicable,
                appliesTo = appliesTo.HasValue ? YesNo(appliesTo.Value) : NotApplicable,
                type = type.GetFriendlyTypeName(true).HtmlEncode(),
                assembly = type.Assembly.GetFriendlyName().HtmlEncode()
            };
        }

        private string YesNo(bool value)
        {
            return value ? "<span class=\"green\">Yes</span>" :
                "<span class=\"red\">No</span>";
        }
    }
}