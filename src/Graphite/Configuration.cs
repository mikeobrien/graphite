using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http.Routing;
using Graphite.Actions;
using Graphite.Authentication;
using Graphite.Behaviors;
using Graphite.Binding;
using Graphite.DependencyInjection;
using Graphite.Diagnostics;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Hosting;
using Graphite.Http;
using Graphite.Readers;
using Graphite.Reflection;
using Graphite.Routing;
using Graphite.Writers;

namespace Graphite
{
    public class Configuration
    {
        public IContainer Container { get; set; }
        public Registry Registry { get; set; } = new Registry();
        public List<Assembly> Assemblies { get; } = new List<Assembly>();

        public string DiagnosticsUrl { get; set; } = "_graphite";
        public bool ReturnErrorMessage { get; set; }
        public bool Diagnostics { get; set; }
        public bool Metrics { get; set; } = true;
        public HttpStatusCode DefaultStatusCode = HttpStatusCode.NoContent;
        public string UnhandledExceptionStatusText { get; set; } =
            "There was a problem processing your request.";
        public bool DefaultErrorHandlerEnabled { get; set; } = true;
        public int DownloadBufferSize { get; set; } = 1024 * 1024;
        public bool AutomaticallyConstrainUrlParameterByType { get; set; }

        public HttpMethods SupportedHttpMethods { get; } = new HttpMethods {
            HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete,
            HttpMethod.Options, HttpMethod.Head, HttpMethod.Trace, HttpMethod.Connect };

        public Plugin<IPathProvider> PathProvider { get; } =
            Plugin<IPathProvider>.Create(singleton: true);

        public Plugin<IInitializer> Initializer { get; } =
            Plugin<IInitializer>
                .Create<Initializer>(singleton: true);

        public Plugin<IInlineConstraintResolver> InlineConstraintResolver { get; } =
            Plugin<IInlineConstraintResolver>
                .Create<DefaultInlineConstraintResolver>(singleton: true);

        public Plugin<IInlineConstraintBuilder> InlineConstraintBuilder { get; } =
            Plugin<IInlineConstraintBuilder>
                .Create<DefaultInlineConstraintBuilder>(singleton: true);

        public Plugin<ITypeCache> TypeCache { get; } =
            Plugin<ITypeCache>
                .Create<TypeCache>(singleton: true);

        public string HandlerNameFilterRegex { get; set; } = "Handler$";

        public Func<Configuration, TypeDescriptor, bool> HandlerFilter { get; set; } =
            (c, t) => t.Name.IsMatch(c.HandlerNameFilterRegex);

        public Func<Configuration, string> ActionRegex { get; set; } =
            c => $"({c.SupportedHttpMethods.Select(m => m.ActionRegex).Join("|")})";

        public Func<Configuration, MethodDescriptor, bool> ActionFilter { get; set; } =
            (c, a) => a.Name.IsMatch(c.ActionRegex(c));

        public Plugins<IActionMethodSource> ActionMethodSources { get; } = 
            new Plugins<IActionMethodSource>(true)
                .Configure(x => x
                    .Append<DefaultActionMethodSource>());

        public Plugins<IActionSource> ActionSources { get; } = 
            new Plugins<IActionSource>(true)
                .Configure(x => x
                    .Append<DiagnosticsActionSource>()
                    .Append<DefaultActionSource>());

        public ConditionalPlugins<IRouteConvention, RouteConfigurationContext> RouteConventions { get; } = 
            new ConditionalPlugins<IRouteConvention, RouteConfigurationContext>(true)
                .Configure(x => x
                    .Append<DefaultRouteConvention>());

        public string HandlerNamespaceRegex { get; set; } = "(.*)";

        public Func<Configuration, ActionMethod, string> GetHandlerNamespace { get; set; } =
            (c, a) => a.HandlerTypeDescriptor.Type.Namespace.MatchGroups(c.HandlerNamespaceRegex).FirstOrDefault();

        public Func<Configuration, ActionMethod, string> GetActionMethodName { get; set; } =
            (c, a) => a.MethodDescriptor.Name.Remove(c.ActionRegex(c));

        public Func<Configuration, ActionMethod, string> GetHttpMethod { get; set; } =
            (c, a) => c.SupportedHttpMethods.MatchAny(a.MethodDescriptor
                .Name.MatchGroups(c.ActionRegex(c)))?.Method;

        public Plugin<IInlineConstraintBuilder> ConstraintBuilder { get; } =
            Plugin<IInlineConstraintBuilder>
                .Create<DefaultInlineConstraintBuilder>(singleton: true);

        public ConditionalPlugins<IUrlConvention, UrlConfigurationContext> UrlConventions { get; } = 
            new ConditionalPlugins<IUrlConvention, UrlConfigurationContext>(true)
                .Configure(x => x
                    .Append<DefaultUrlConvention>()
                    .Append<AliasUrlConvention>());

        public List<Func<ActionMethod, Url, string>> UrlAliases { get; } =
            new List<Func<ActionMethod, Url, string>>();
        public string UrlPrefix { get; set; }

        public ConditionalPlugins<IActionDecorator, ActionConfigurationContext> ActionDecorators { get; } = 
            new ConditionalPlugins<IActionDecorator, ActionConfigurationContext>(true);

        public ConditionalPlugins<IHttpRouteDecorator, ActionConfigurationContext> HttpRouteDecorators { get; } = 
            new ConditionalPlugins<IHttpRouteDecorator, ActionConfigurationContext>(true);

        public Plugin<IHttpRouteMapper> HttpRouteMapper { get; } =
            Plugin<IHttpRouteMapper>
                .Create<HttpRouteMapper>(singleton: true);

        public Plugin<IBehaviorChainInvoker> BehaviorChainInvoker { get; } =
            Plugin<IBehaviorChainInvoker>
                .Create<BehaviorChainInvoker>();

        public Plugin<IActionInvoker> ActionInvoker { get; } =
            Plugin<IActionInvoker>
                .Create<ActionInvoker>();
        
        public Plugin<IUnhandledExceptionHandler> UnhandledExceptionHandler { get; } =
            Plugin<IUnhandledExceptionHandler>
                .Create<UnhandledExceptionHandler>();

        public Plugin<IRequestPropertiesProvider> RequestPropertiesProvider { get; } =
            Plugin<IRequestPropertiesProvider>.Create();

        public string DefaultAuthenticationRealm { get; set; }
        public string DefaultUnauthorizedStatusMessage { get; set; }

        public Type BehaviorChain { get; set; } = typeof(BehaviorChain);
        public Type DefaultBehavior { get; set; } = typeof(InvokerBehavior);
        public BindingMode HeadersBindingMode { get; set; } = BindingMode.None;
        public BindingMode CookiesBindingMode { get; set; } = BindingMode.None;
        public BindingMode RequestInfoBindingMode { get; set; } = BindingMode.None;
        public BindingMode ContinerBindingMode { get; set; } = BindingMode.None;
        public bool BindComplexTypeProperties { get; set; }

        public ConditionalPlugins<IValueMapper, ValueMapperConfigurationContext> ValueMappers { get; } = 
            new ConditionalPlugins<IValueMapper, ValueMapperConfigurationContext>(false)
            .Configure(x => x
                .Append<SimpleTypeMapper>());

        // Action scoped configuration

        public ConditionalPlugins<IAuthenticator, ActionConfigurationContext> Authenticators { get; } = 
            new ConditionalPlugins<IAuthenticator, ActionConfigurationContext>(false);

        public ConditionalPlugins<IRequestBinder, ActionConfigurationContext> RequestBinders { get; } = 
            new ConditionalPlugins<IRequestBinder, ActionConfigurationContext>(false)
            .Configure(x => x
                .Append<ReaderBinder>()
                .Append<UrlParameterBinder>()
                .Append<QuerystringBinder>()
                .Append<FormBinder>()
                .Append<JsonBinder>()
                .Append<XmlBinder>()
                .Append<HeaderBinder>()
                .Append<CookieBinder>()
                .Append<RequestPropertiesBinder>()
                .Append<ContainerBinder>());

        public ConditionalPlugins<IRequestReader, ActionConfigurationContext> RequestReaders { get; } =
            new ConditionalPlugins<IRequestReader, ActionConfigurationContext>(false)
            .Configure(x => x
                .Append<StringReader>()
                .Append<StreamReader>()
                .Append<ByteReader>()
                .Append<JsonReader>()
                .Append<XmlReader>()
                .Append<FormReader>());

        public ConditionalPlugins<IResponseWriter, ActionConfigurationContext> ResponseWriters { get; } =
            new ConditionalPlugins<IResponseWriter, ActionConfigurationContext>(false)
            .Configure(x => x
                .Append<RedirectWriter>()
                .Append<StringWriter>()
                .Append<StreamWriter>()
                .Append<ByteWriter>()
                .Append<JsonWriter>()
                .Append<XmlWriter>());

        public ConditionalPlugins<IBehavior, ActionConfigurationContext> Behaviors { get; } = 
            new ConditionalPlugins<IBehavior, ActionConfigurationContext>(false)
            .Configure(x => x
                .Append<DefaultErrorHandlerBehavior>(y => y
                    .Configuration.DefaultErrorHandlerEnabled));
    }
}
