using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Xml;
using Graphite.Actions;
using Graphite.Authentication;
using Graphite.Behaviors;
using Graphite.Binding;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Hosting;
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
        private readonly HttpConfiguration _httpConfiguration;

        public ConfigurationDsl(Configuration configuration, HttpConfiguration httpConfiguration)
        {
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
        }

        /// <summary>
        /// Allows you to configure Web Api.
        /// </summary>
        public ConfigurationDsl ConfigureWebApi(Action<HttpConfiguration> configure)
        {
            configure(_httpConfiguration);
            return this;
        }

        public class XmlSerializationDsl
        {
            private readonly Configuration _configuration;

            public XmlSerializationDsl(Configuration configuration)
            {
                _configuration = configuration;
            }

            /// <summary>
            /// Allows you to configure the XML writer.
            /// </summary>
            public XmlSerializationDsl Writer(Action<XmlWriterSettings> configure)
            {
                configure?.Invoke(_configuration.XmlWriterSettings);
                return this;
            }

            /// <summary>
            /// Allows you to configure the XML reader.
            /// </summary>
            public XmlSerializationDsl Reader(Action<XmlReaderSettings> configure)
            {
                configure?.Invoke(_configuration.XmlReaderSettings);
                return this;
            }
        }

        public class SerializationDsl
        {
            private readonly Configuration _configuration;

            public SerializationDsl(Configuration configuration)
            {
                _configuration = configuration;
            }

            /// <summary>
            /// Allows you to configure Json.NET.
            /// </summary>
            public SerializationDsl Json(Action<JsonSerializerSettings> configure)
            {
                configure?.Invoke(_configuration.JsonSerializerSettings);
                return this;
            }

            /// <summary>
            /// Allows you to configure the XML serializer.
            /// </summary>
            public SerializationDsl Xml(Action<XmlSerializationDsl> configure)
            {
                configure?.Invoke(new XmlSerializationDsl(_configuration));
                return this;
            }
        }

        /// <summary>
        /// Allows you to configure serialization.
        /// </summary>
        public ConfigurationDsl ConfigureSerialization(Action<SerializationDsl> configure)
        {
            configure?.Invoke(new SerializationDsl(_configuration));
            return this;
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
        /// Includes the current assemby.
        /// </summary>
        public ConfigurationDsl IncludeThisAssembly()
        {
            IncludeAssemblies(Assembly.GetCallingAssembly());
            return this;
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
            _configuration.Metrics = false;
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessages()
        {
            ReturnErrorMessagesWhen(x => true);
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions
        /// when the predicate is true.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessagesWhen(Func<HttpRequestMessage, bool> predicate)
        {
            _configuration.ReturnErrorMessage = predicate;
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions
        /// when calling assembly is in debug mode.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessagesInDebugMode()
        {
            var debugMode = Assembly.GetCallingAssembly().IsInDebugMode();
            ReturnErrorMessagesWhen(x => debugMode);
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions
        /// when calling assembly is in debug mode 
        /// or the predicate is true.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessagesInDebugModeOrWhen(
            Func<HttpRequestMessage, bool> predicate)
        {
            var debugMode = Assembly.GetCallingAssembly().IsInDebugMode();
            ReturnErrorMessagesWhen(x => debugMode || predicate(x));
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions
        /// when type assembly is in debug mode.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessagesInDebugMode<T>()
        {
            var debugMode = typeof(T).Assembly.IsInDebugMode();
            ReturnErrorMessagesWhen(x => debugMode);
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions
        /// when type assembly is in debug mode
        /// or the predicate is true.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessagesInDebugModeOrWhen<T>(
            Func<HttpRequestMessage, bool> predicate)
        {
            var debugMode = typeof(T).Assembly.IsInDebugMode();
            ReturnErrorMessagesWhen(x => debugMode || predicate(x));
            return this;
        }

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
        /// Indicates that responses should be disposed 
        /// if they implement IDisposable.
        /// </summary>
        public ConfigurationDsl DisposeResponses()
        {
            _configuration.DisposeResponses = true;
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
        /// Specifies the default status code, the default is 204 (no content).
        /// </summary>
        public ConfigurationDsl WithDefaultStatusCode(HttpStatusCode statusCode)
        {
            _configuration.DefaultStatusCode = statusCode;
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
        /// Specifies the default encoding.
        /// </summary>
        public ConfigurationDsl WithDefaultEncoding<T>(T encoding) where T : Encoding
        {
            _configuration.DefaultEncoding = encoding;
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
        /// Specifies the path provider to use.
        /// </summary>
        public ConfigurationDsl WithPathProvider<T>() where T : IPathProvider
        {
            _configuration.PathProvider.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the path provider to use.
        /// </summary>
        public ConfigurationDsl WithPathProvider<T>(T instance) where T : IPathProvider
        {
            _configuration.PathProvider.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the request properties provider to use.
        /// </summary>
        public ConfigurationDsl WithRequestPropertyProvider<T>() where T : IRequestPropertiesProvider
        {
            _configuration.RequestPropertiesProvider.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the request properties provider to use.
        /// </summary>
        public ConfigurationDsl WithRequestPropertyProvider<T>(T instance) where T : IRequestPropertiesProvider
        {
            _configuration.RequestPropertiesProvider.Set(instance);
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
        /// Specifies the route mapper to use.
        /// </summary>
        public ConfigurationDsl WithRouteMapper<T>() where T : IHttpRouteMapper
        {
            _configuration.HttpRouteMapper.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the route mapper to use.
        /// </summary>
        public ConfigurationDsl WithRouteMapper<T>(T instance) where T : IHttpRouteMapper
        {
            _configuration.HttpRouteMapper.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint resolver to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintResolver<T>() where T : IInlineConstraintResolver
        {
            _configuration.InlineConstraintResolver.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint resolver to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintResolver<T>(T instance) where T : IInlineConstraintResolver
        {
            _configuration.InlineConstraintResolver.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint builder to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintBuilder<T>() where T : IInlineConstraintBuilder
        {
            _configuration.InlineConstraintBuilder.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint builder to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintBuilder<T>(T instance) where T : IInlineConstraintBuilder
        {
            _configuration.InlineConstraintBuilder.Set(instance);
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
        /// Specifies the catch all exception handler. This is NOT something 
        /// you would generally override and is NOT the appropriate place for 
        /// app and request error handling and logging. That should be done 
        /// at the app level (e.g. Global.asax) and with a behavior respectively.
        /// </summary>
        public ConfigurationDsl WithUnhandledExceptionHandler<T>() where T : IUnhandledExceptionHandler
        {
            _configuration.UnhandledExceptionHandler.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the catch all exception handler. This is NOT something 
        /// you would generally override and is NOT the appropriate place for 
        /// app and request error handling and logging. That should be done 
        /// at the app level (e.g. Global.asax) and with a behavior respectively.
        /// </summary>
        public ConfigurationDsl WithUnhandledExceptionHandler<T>(T instance) where T : IUnhandledExceptionHandler
        {
            _configuration.UnhandledExceptionHandler.Set(instance);
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
        /// Specifies the behavior chain to use.
        /// </summary>
        public ConfigurationDsl WithBehaviorChain<T>() where T : IBehaviorChain
        {
            _configuration.BehaviorChain = typeof(T);
            return this;
        }

        /// <summary>
        /// Specifies the last behavior in the chain.
        /// </summary>
        public ConfigurationDsl WithDefaultBehavior<T>() where T : IBehavior
        {
            _configuration.DefaultBehavior = typeof(T);
            return this;
        }

        /// <summary>
        /// Configure http methods.
        /// </summary>
        public ConfigurationDsl ConfigureHttpMethods(Action<HttpMethods> configure)
        {
            configure(_configuration.SupportedHttpMethods);
            return this;
        }

        /// <summary>
        /// Adds url aliases.
        /// </summary>
        public ConfigurationDsl WithUrlAlias(params Func<ActionMethod, Url, string>[] aliases)
        {
            _configuration.UrlAliases.AddRange(aliases);
            return this;
        }

        /// <summary>
        /// Adds a prefix to all urls.
        /// </summary>
        public ConfigurationDsl WithUrlPrefix(string prefix)
        {
            _configuration.UrlPrefix = prefix;
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
        /// Removes the types namespace from the url.
        /// </summary>
        public ConfigurationDsl ExcludeTypeNamespaceFromUrl<T>()
        {
            return ExcludeTypeNamespaceFromUrl(typeof(T));
        }

        /// <summary>
        /// Removes the calling method's type namespace from the url.
        /// </summary>
        public ConfigurationDsl ExcludeCurrentNamespaceFromUrl()
        {
            return ExcludeTypeNamespaceFromUrl(new StackFrame(1).GetMethod().ReflectedType);
        }

        /// <summary>
        /// Removes the types namespace from the url.
        /// </summary>
        public ConfigurationDsl ExcludeTypeNamespaceFromUrl(Type type)
        {
            return ExcludeNamespaceFromUrl(type.Namespace);
        }

        /// <summary>
        /// Removes the namespace from the begining of the url.
        /// </summary>
        public ConfigurationDsl ExcludeNamespaceFromUrl(string @namespace)
        {
            _configuration.HandlerNamespaceRegex = $"{@namespace.RegexEscape()}\\.?(.*)";
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
        public ConfigurationDsl ConfigureActionMethodSources(
            Action<PluginsDsl<IActionMethodSource>> configure)
        {
            _configuration.ActionMethodSources.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures action sources.
        /// </summary>
        public ConfigurationDsl ConfigureActionSources(
            Action<PluginsDsl<IActionSource>> configure)
        {
            _configuration.ActionSources.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures action decorators.
        /// </summary>
        public ConfigurationDsl ConfigureActionDecorators(Action<ConditionalPluginsDsl
            <IActionDecorator, ActionConfigurationContext>> configure)
        {
            _configuration.ActionDecorators.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures route conventions.
        /// </summary>
        public ConfigurationDsl ConfigureRouteConventions(Action<ConditionalPluginsDsl
            <IRouteConvention, RouteConfigurationContext>> configure)
        {
            _configuration.RouteConventions.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures url conventions.
        /// </summary>
        public ConfigurationDsl ConfigureUrlConventions(Action<ConditionalPluginsDsl
            <IUrlConvention, UrlConfigurationContext>> configure)
        {
            _configuration.UrlConventions.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures http route decorators.
        /// </summary>
        public ConfigurationDsl ConfigureHttpRouteDecorators(Action<ConditionalPluginsDsl
            <IHttpRouteDecorator, ActionConfigurationContext>> configure)
        {
            _configuration.HttpRouteDecorators.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures request readers.
        /// </summary>
        public ConfigurationDsl ConfigureRequestReaders(Action<ConditionalPluginsDsl
            <IRequestReader, ActionConfigurationContext>> configure)
        {
            _configuration.RequestReaders.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures request binders.
        /// </summary>
        public ConfigurationDsl ConfigureRequestBinders(Action<ConditionalPluginsDsl
            <IRequestBinder, ActionConfigurationContext>> configure)
        {
            _configuration.RequestBinders.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures value mappers.
        /// </summary>
        public ConfigurationDsl ConfigureValueMappers(Action<ConditionalPluginsDsl
            <IValueMapper, ValueMapperConfigurationContext>> configure)
        {
            _configuration.ValueMappers.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures response writers.
        /// </summary>
        public ConfigurationDsl ConfigureResponseWriters(Action<ConditionalPluginsDsl
            <IResponseWriter, ActionConfigurationContext>> configure)
        {
            _configuration.ResponseWriters.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures behaviors.
        /// </summary>
        public ConfigurationDsl ConfigureBehaviors(Action<ConditionalPluginsDsl
            <IBehavior, ActionConfigurationContext>> configure)
        {
            _configuration.Behaviors.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures authenticators.
        /// </summary>
        public ConfigurationDsl ConfigureAuthenticators(Action<ConditionalPluginsDsl
            <IAuthenticator, ActionConfigurationContext>> configure)
        {
            _configuration.Authenticators.Configure(configure);
            return this;
        }

        /// <summary>
        /// Sets the default authentication realm.
        /// </summary>
        public ConfigurationDsl WithDefaultAuthenticationRealm(string realm)
        {
            _configuration.DefaultAuthenticationRealm = realm;
            return this;
        }

        /// <summary>
        /// Sets the default unauthorized status message.
        /// </summary>
        public ConfigurationDsl WithDefaultUnauthorizedStatusMessage(string statusMessage)
        {
            _configuration.DefaultUnauthorizedStatusMessage = statusMessage;
            return this;
        }

        /// <summary>
        /// Binds header values to action parameters.
        /// </summary>
        public ConfigurationDsl BindHeaders()
        {
            _configuration.HeadersBindingMode = BindingMode.Implicit;
            return this;
        }

        /// <summary>
        /// Binds header values to action parameters by convention.
        /// </summary>
        public ConfigurationDsl BindHeadersByNamingConvention()
        {
            _configuration.HeadersBindingMode = BindingMode.Convention;
            return this;
        }

        /// <summary>
        /// Binds header values to action parameters by attribute.
        /// </summary>
        public ConfigurationDsl BindHeadersByAttribute()
        {
            _configuration.HeadersBindingMode = BindingMode.Explicit;
            return this;
        }

        /// <summary>
        /// Binds cookie values to action parameters.
        /// </summary>
        public ConfigurationDsl BindCookies()
        {
            _configuration.CookiesBindingMode = BindingMode.Implicit;
            return this;
        }

        /// <summary>
        /// Binds cookie values to action parameters by convention.
        /// </summary>
        public ConfigurationDsl BindCookiesByNamingConvention()
        {
            _configuration.CookiesBindingMode = BindingMode.Convention;
            return this;
        }

        /// <summary>
        /// Binds cookie values to action parameters by attribute.
        /// </summary>
        public ConfigurationDsl BindCookiesByAttribute()
        {
            _configuration.CookiesBindingMode = BindingMode.Explicit;
            return this;
        }

        /// <summary>
        /// Binds request info values to action parameters.
        /// </summary>
        public ConfigurationDsl BindRequestInfo()
        {
            _configuration.RequestInfoBindingMode = BindingMode.Implicit;
            return this;
        }

        /// <summary>
        /// Binds request info values to action parameters by convention.
        /// </summary>
        public ConfigurationDsl BindRequestInfoByAttribute()
        {
            _configuration.RequestInfoBindingMode = BindingMode.Explicit;
            return this;
        }

        /// <summary>
        /// Binds container values to action parameters.
        /// </summary>
        public ConfigurationDsl BindContainer()
        {
            _configuration.ContinerBindingMode = BindingMode.Implicit;
            return this;
        }

        /// <summary>
        /// Binds container values to action parameters by attribute.
        /// </summary>
        public ConfigurationDsl BindContainerByAttribute()
        {
            _configuration.ContinerBindingMode = BindingMode.Explicit;
            return this;
        }

        /// <summary>
        /// Binds request parameters to the first level of 
        /// properties of a complex action parameter type.
        /// </summary>
        public ConfigurationDsl BindComplexTypeProperties()
        {
            _configuration.BindComplexTypeProperties = true;
            return this;
        }

        /// <summary>
        /// Automatically constrain url parameters by type.
        /// </summary>
        public ConfigurationDsl AutomaticallyConstrainUrlParameterByType()
        {
            _configuration.AutomaticallyConstrainUrlParameterByType = true;
            return this;
        }
    }
}
