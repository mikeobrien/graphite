using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http.Routing;
using System.Xml;
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
using Newtonsoft.Json;
using HttpMethod = Graphite.Http.HttpMethod;
using JsonReader = Graphite.Readers.JsonReader;
using JsonWriter = Graphite.Writers.JsonWriter;
using XmlReader = Graphite.Readers.XmlReader;
using XmlWriter = Graphite.Writers.XmlWriter;

namespace Graphite
{
    public class Configuration
    {
        public IContainer Container { get; set; }
        public Registry Registry { get; set; } = new Registry(new TypeCache());
        public List<Assembly> Assemblies { get; } = new List<Assembly>();
        public Encoding DefaultEncoding { get; set; } = new UTF8Encoding(false);

        public string DiagnosticsUrl { get; set; } = "_graphite";
        public bool Diagnostics { get; set; }
        public bool Metrics { get; set; } = true;

        public HttpStatusCode DefaultBindingFailureStatusCode { get; set; } = HttpStatusCode.BadRequest;
        public Func<string, string> DefaultBindingFailureStatusText { get; set; } = x => x;
        public HttpStatusCode DefaultNoReaderStatusCode { get; set; } = HttpStatusCode.BadRequest;
        public string DefaultNoReaderStatusText { get; set; } = "Request format not supported or not specified.";
        public HttpStatusCode DefaultHasResponseStatusCode { get; set; } = HttpStatusCode.OK;
        public string DefaultHasResponseStatusText { get; set; }
        public HttpStatusCode DefaultNoResponseStatusCode { get; set; } = HttpStatusCode.NoContent;
        public string DefaultNoResponseStatusText { get; set; }
        public HttpStatusCode DefaultNoWriterStatusCode { get; set; } = HttpStatusCode.BadRequest;
        public string DefaultNoWriterStatusText { get; set; } = "Response format not supported or not specified.";

        public int DownloadBufferSize { get; set; } = 1.MB();
        public int? SerializerBufferSize { get; set; }
        public bool AutomaticallyConstrainUrlParameterByType { get; set; }
        public bool DisposeSerializedObjects { get; set; }
        public bool FailIfNoAuthenticatorsApplyToAction { get; set; } = true;
        public bool ExcludeDiagnosticsFromAuthentication { get; set; }
        public bool FailIfNoMapperFound { get; set; }

        public XmlReaderSettings XmlReaderSettings { get; } = new XmlReaderSettings();
        public XmlWriterSettings XmlWriterSettings { get; } = new XmlWriterSettings();
        public JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings();

        public HttpMethods SupportedHttpMethods { get; } = new HttpMethods
        {
            HttpMethod.Get, HttpMethod.Post, HttpMethod.Put,
            HttpMethod.Patch, HttpMethod.Delete, HttpMethod.Options, 
            HttpMethod.Head, HttpMethod.Trace, HttpMethod.Connect
        };

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

        public Plugin<IInlineConstraintBuilder> ConstraintBuilder { get; } =
            Plugin<IInlineConstraintBuilder>
                .Create<DefaultInlineConstraintBuilder>(singleton: true);
        
        public Regex HandlerNameConvention { get; set; }
        public Func<Configuration, TypeDescriptor, bool> HandlerFilter { get; set; }
        public Func<Configuration, Regex> ActionNameConvention { get; set; }
        public Func<Configuration, MethodDescriptor, bool> ActionFilter { get; set; }
        public Func<Configuration, ActionMethod, string[]> ActionSegmentsConvention { get; set; }
        public Func<Configuration, ActionMethod, string> HttpMethodConvention { get; set; }

        public ConditionalPlugins<IUrlConvention, UrlContext> UrlConventions { get; } = 
            new ConditionalPlugins<IUrlConvention, UrlContext>(true)
                .Configure(x => x
                    .Append<DefaultUrlConvention>());

        public List<NamespaceMapping> NamespaceUrlMappings { get; set; } = 
            new List<NamespaceMapping>
            {
                NamespaceMapping.DefaultMapping
            };

        public List<Func<UrlContext, string>> UrlAliases { get; } =
            new List<Func<UrlContext, string>>();
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
        public bool CreateEmptyRequestParameterValue { get; set; } = true;

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

        public ConditionalPlugins<IResponseStatus, ActionConfigurationContext> ResponseStatus { get; } =
            new ConditionalPlugins<IResponseStatus, ActionConfigurationContext>(false)
                .Configure(x => x
                    .Append<DefaultResponseStatus>(@default: true));

        public ConditionalPlugins<IBehavior, ActionConfigurationContext> Behaviors { get; } = 
            new ConditionalPlugins<IBehavior, ActionConfigurationContext>(false);
    }
}
