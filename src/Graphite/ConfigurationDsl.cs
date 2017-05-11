using System;
using System.Linq;
using System.Reflection;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Readers;
using Graphite.Reflection;
using Graphite.Routing;
using Graphite.Writers;
using Newtonsoft.Json;

namespace Graphite
{
    public class ConfigurationDsl
    {
        private readonly Configuration _configuration;

        public ConfigurationDsl(Configuration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Allows you to configure Json.NET.
        /// </summary>
        public ConfigurationDsl ConfigureJsonNet(Action<JsonSerializerSettings> configure)
        {
            var settings = new JsonSerializerSettings();
            configure?.Invoke(settings);
            return ConfigureRegistry(x => x.Register(settings));
        }

        /// <summary>
        /// Includes the assembly of the specified type.
        /// This call is additive, so you can specify multiple assemblies.
        /// </summary>
        public ConfigurationDsl IncludeTypeAssembly<T>()
        {
            IncludeTypeAssembly(typeof(T));
            return this;
        }

        /// <summary>
        /// Includes the assembly of the specified type.
        /// This call is additive, so you can specify multiple assemblies..
        /// </summary>
        public ConfigurationDsl IncludeTypeAssembly(Type type)
        {
            return IncludeAssemblies(type.Assembly);
        }

        /// <summary>
        /// Includes the specified assemblies.
        /// </summary>
        public ConfigurationDsl IncludeAssemblies(params Assembly[] assemblies)
        {
            _configuration.Assemblies.AddRange(assemblies
                .Where(x => !_configuration.Assemblies.Contains(x)));
            return this;
        }

        /// <summary>
        /// Clears the default assemblies.
        /// </summary>
        public ConfigurationDsl ClearAssemblies()
        {
            _configuration.Assemblies.Clear();
            return this;
        }

        /// <summary>
        /// Disables the built in metrics.
        /// </summary>
        public ConfigurationDsl DisableMetrics()
        {
            _configuration.EnableMetrics = false;
            return this;
        }

        /// <summary>
        /// Enables the diagnostics page and returns 
        /// the stack trace of unhandled exceptions.
        /// </summary>
        public ConfigurationDsl EnableDiagnostics()
        {
            _configuration.EnableDiagnostics = true;
            return this;
        }

        /// <summary>
        /// Enables the diagnostics page and returns 
        /// the stack trace of unhandled exceptions
        /// when calling assembly is in debug mode.
        /// </summary>
        public ConfigurationDsl EnableDiagnosticsInDebugMode()
        {
            _configuration.EnableDiagnostics = Assembly
                .GetCallingAssembly().IsInDebugMode();
            return this;
        }

        /// <summary>
        /// Enables the diagnostics page and returns 
        /// the stack trace of unhandled exceptions
        /// when type assembly is in debug mode.
        /// </summary>
        public ConfigurationDsl EnableDiagnosticsInDebugMode<T>()
        {
            _configuration.EnableDiagnostics = typeof(T).Assembly.IsInDebugMode();
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
        /// Specifies the download buffer size in bytes. The default is 1MB.
        /// </summary>
        public ConfigurationDsl WithDownloadBufferSizeOf(int length)
        {
            _configuration.DownloadBufferSize = length;
            return this;
        }

        /// <summary>
        /// Specifies the status text returned by an unhandled exception.
        /// </summary>
        public ConfigurationDsl WithUnhandledExceptionStatusText(string statusText)
        {
            _configuration.UnhandledExceptionStatusText = statusText;
            return this;
        }

        /// <summary>
        /// Disables the default error handler.
        /// </summary>
        public ConfigurationDsl DisableDefaultErrorHandler()
        {
            _configuration.DefaultErrorHandlerEnabled = false;
            return this;
        }

        /// <summary>
        /// Specifies the IoC container to use.
        /// </summary>
        public ConfigurationDsl UseContainer(IContainer container)
        {
            _configuration.Container = container;
            return this;
        }

        /// <summary>
        /// Specifies the IoC container to use.
        /// </summary>
        public ConfigurationDsl UseContainer<T>() where T : IContainer, new()
        {
            _configuration.Container = new T();
            return this;
        }

        /// <summary>
        /// Specifies the initializer to use.
        /// </summary>
        public ConfigurationDsl WithInitializer<T>() where T : IInitializer
        {
            _configuration.Initializer.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the initializer to use.
        /// </summary>
        public ConfigurationDsl WithInitializer<T>(T instance) where T : IInitializer
        {
            _configuration.Initializer.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the type cache to use.
        /// </summary>
        public ConfigurationDsl WithTypeCache<T>() where T : ITypeCache
        {
            _configuration.TypeCache.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the type cache to use.
        /// </summary>
        public ConfigurationDsl WithTypeCache<T>(T instance) where T : ITypeCache
        {
            _configuration.TypeCache.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the action invoker to use.
        /// </summary>
        public ConfigurationDsl WithActionInvoker<T>() where T : IActionInvoker
        {
            _configuration.ActionInvoker.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the action invoker to use.
        /// </summary>
        public ConfigurationDsl WithActionInvoker<T>(T instance) where T : IActionInvoker
        {
            _configuration.ActionInvoker.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the behavior chain invoker to use.
        /// </summary>
        public ConfigurationDsl WithBehaviorChainInvoker<T>() where T : IBehaviorChainInvoker
        {
            _configuration.BehaviorChainInvoker.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the behavior chain invoker to use.
        /// </summary>
        public ConfigurationDsl WithBehaviorChainInvoker<T>(T instance) where T : IBehaviorChainInvoker
        {
            _configuration.BehaviorChainInvoker.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the invoker behavior to use.
        /// </summary>
        public ConfigurationDsl WithInvokerBehavior<T>() where T : IInvokerBehavior
        {
            _configuration.InvokerBehavior.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the invoker behavior to use.
        /// </summary>
        public ConfigurationDsl WithInvokerBehavior<T>(T instance) where T : IInvokerBehavior
        {
            _configuration.InvokerBehavior.Set(instance);
            return this;
        }

        /// <summary>
        /// Adds an http method.
        /// </summary>
        public ConfigurationDsl AddHttpMethod(string method, string actionRegex, 
            bool allowRequestbody, bool allowResponsebody)
        {
            _configuration.SupportedHttpMethods.Add(new HttpMethod(actionRegex, 
                method, allowRequestbody, allowResponsebody));
            return this;
        }

        /// <summary>
        /// Removes an http method.
        /// </summary>
        public ConfigurationDsl RemoveHttpMethod(params string[] methods)
        {
            methods.ForEach(method =>
            {
                var remove = _configuration.SupportedHttpMethods.FirstOrDefault(
                    x => x.Method.EqualsIgnoreCase(method));
                if (remove != null) _configuration.SupportedHttpMethods.Remove(remove);
            });
            return this;
        }

        /// <summary>
        /// Removes an http method.
        /// </summary>
        public ConfigurationDsl RemoveHttpMethod(params HttpMethod[] methods)
        {
            methods.ForEach(method => _configuration.SupportedHttpMethods.Remove(method));
            return this;
        }

        /// <summary>
        /// Clears all default http methods.
        /// </summary>
        public ConfigurationDsl ClearHttpMethods()
        {
            _configuration.SupportedHttpMethods.Clear();
            return this;
        }

        /// <summary>
        /// Adds url aliases.
        /// </summary>
        public ConfigurationDsl WithUrlAlias(params Func<ActionMethod, string[], string>[] aliases)
        {
            _configuration.UrlAliases.AddRange(aliases);
            return this;
        }

        /// <summary>
        /// Specifies the handler filter.
        /// </summary>
        public ConfigurationDsl FilterHandlersBy(Func<Configuration, TypeDescriptor, bool> filter)
        {
            _configuration.HandlerFilter = filter;
            return this;
        }

        /// <summary>
        /// Only includes handlers under the namespace of the specified type.
        /// </summary>
        public ConfigurationDsl OnlyIncludeHandlersUnder<T>()
        {
            _configuration.HandlerFilter = (c, t) =>
                t.Type.IsUnderNamespace<T>() &&
                t.Name.IsMatch(c.HandlerNameFilterRegex);
            return this;
        }

        /// <summary>
        /// Specifies the regex used to identify handlers e.g. "Handler$".
        /// </summary>
        public ConfigurationDsl WithHandlerNameRegex(string regex)
        {
            _configuration.HandlerNameFilterRegex = regex;
            return this;
        }

        /// <summary>
        /// Specifies the action filter.
        /// </summary>
        public ConfigurationDsl FilterActionsBy(Func<Configuration, MethodDescriptor, bool> filter)
        {
            _configuration.ActionFilter = filter;
            return this;
        }

        /// <summary>
        /// Specifies the regex used to identify actions. The http method is 
        /// pulled from the first capture group by default e.g. "^(Get|Post|...)".
        /// </summary>
        public ConfigurationDsl WithActionRegex(Func<Configuration, string> regex)
        {
            _configuration.ActionRegex = regex;
            return this;
        }

        /// <summary>
        /// Specifies the regex used to parse the handler namespace. The namespace is 
        /// pulled from the first capture group by default e.g. "MyApp\.Handlers\.(.*)".
        /// </summary>
        public ConfigurationDsl WithHandlerNamespaceRegex(string regex)
        {
            _configuration.HandlerNamespaceRegex = regex;
            return this;
        }

        /// <summary>
        /// Specifies the regex used to parse the handler namespace. The namespace is 
        /// pulled from the first capture group by default e.g. "MyApp\.Handlers\.(.*)".
        /// </summary>
        public ConfigurationDsl ExcludeTypeNamespaceFromUrl<T>()
        {
            return ExcludeTypeNamespaceFromUrl(typeof(T));
        }

        /// <summary>
        /// Specifies the regex used to parse the handler namespace. The namespace is 
        /// pulled from the first capture group by default e.g. "MyApp\.Handlers\.(.*)".
        /// </summary>
        public ConfigurationDsl ExcludeTypeNamespaceFromUrl(Type type)
        {
            _configuration.HandlerNamespaceRegex = $"{type.Namespace}\\.?(.*)";
            return this;
        }

        /// <summary>
        /// Gets the portion of the action method name used for routing.
        /// </summary>
        public ConfigurationDsl GetActionMethodNameWith(Func<Configuration, ActionMethod, string> getName)
        {
            _configuration.GetActionMethodName = getName;
            return this;
        }

        /// <summary>
        /// Gets the http method from the action method name.
        /// </summary>
        public ConfigurationDsl GetHttpMethodWith(Func<Configuration, ActionMethod, string> getMethod)
        {
            _configuration.GetHttpMethod = getMethod;
            return this;
        }

        /// <summary>
        /// Configures the IoC container.
        /// </summary>
        public ConfigurationDsl ConfigureRegistry(Action<Registry> configure)
        {
            configure(_configuration.Registry);
            return this;
        }

        /// <summary>
        /// Configures action method sources.
        /// </summary>
        public ConfigurationDsl ConfigureActionMethodSources(Action<PluginDefinitions
            <IActionMethodSource, ActionMethodSourceContext>> configure)
        {
            configure(_configuration.ActionMethodSources);
            return this;
        }

        /// <summary>
        /// Configures action sources.
        /// </summary>
        public ConfigurationDsl ConfigureActionSources(Action<PluginDefinitions
            <IActionSource, ActionSourceContext>> configure)
        {
            configure(_configuration.ActionSources);
            return this;
        }

        /// <summary>
        /// Configures route conventions.
        /// </summary>
        public ConfigurationDsl ConfigureRouteConventions(Action<PluginDefinitions
            <IRouteConvention, RouteContext>> configure)
        {
            configure(_configuration.RouteConventions);
            return this;
        }

        /// <summary>
        /// Configures url conventions.
        /// </summary>
        public ConfigurationDsl ConfigureUrlConventions(Action<PluginDefinitions
            <IUrlConvention, UrlContext>> configure)
        {
            configure(_configuration.UrlConventions);
            return this;
        }

        /// <summary>
        /// Configures request readers.
        /// </summary>
        public ConfigurationDsl ConfigureRequestReaders(Action<PluginDefinitions
            <IRequestReader, RequestReaderContext>> configure)
        {
            configure(_configuration.RequestReaders);
            return this;
        }

        /// <summary>
        /// Configures request binders.
        /// </summary>
        public ConfigurationDsl ConfigureRequestBinders(Action<PluginDefinitions
            <IRequestBinder, RequestBinderContext>> configure)
        {
            configure(_configuration.RequestBinders);
            return this;
        }

        /// <summary>
        /// Configures value mappers.
        /// </summary>
        public ConfigurationDsl ConfigureValueMappers(Action<PluginDefinitions
            <IValueMapper, ValueMapperContext>> configure)
        {
            configure(_configuration.ValueMappers);
            return this;
        }

        /// <summary>
        /// Configures response writers.
        /// </summary>
        public ConfigurationDsl ConfigureResponseWriters(Action<PluginDefinitions
            <IResponseWriter, ResponseWriterContext>> configure)
        {
            configure(_configuration.ResponseWriters);
            return this;
        }

        /// <summary>
        /// Configures behaviors.
        /// </summary>
        public ConfigurationDsl ConfigureBehaviors(Action<PluginDefinitions
            <IBehavior, BehaviorContext>> configure)
        {
            configure(_configuration.Behaviors);
            return this;
        }
    }
}
