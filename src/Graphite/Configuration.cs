using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http.Routing;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Binding;
using Graphite.DependencyInjection;
using Graphite.Diagnostics;
using Graphite.Extensibility;
using Graphite.Extensions;
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

        public PluginDefinition<IInitializer> Initializer { get; } =
            PluginDefinition<IInitializer>
                .Create<Initializer>();

        public PluginDefinition<IRouteMapper> RouteMapper { get; } =
            PluginDefinition<IRouteMapper>
                .Create<RouteMapper>();

        public PluginDefinition<IInlineConstraintResolver> InlineConstraintResolver { get; } =
            PluginDefinition<IInlineConstraintResolver>
                .Create<DefaultInlineConstraintResolver>();

        public PluginDefinition<IInlineConstraintBuilder> InlineConstraintBuilder { get; } =
            PluginDefinition<IInlineConstraintBuilder>
                .Create<DefaultInlineConstraintBuilder>();

        public PluginDefinition<ITypeCache> TypeCache { get; } =
            PluginDefinition<ITypeCache>
                .Create<TypeCache>();
        
        public PluginDefinitions<IActionMethodSource, ConfigurationContext> ActionMethodSources { get; } =
            PluginDefinitions<IActionMethodSource, ConfigurationContext>.Create(x => x
                .Append<DefaultActionMethodSource>());

        public PluginDefinitions<IActionSource, ConfigurationContext> ActionSources { get; } =
            PluginDefinitions<IActionSource, ConfigurationContext>.Create(x => x
                .Append<DiagnosticsActionSource>()
                .Append<DefaultActionSource>());

        public PluginDefinitions<IRouteConvention, RouteConfigurationContext> RouteConventions { get; } =
            PluginDefinitions<IRouteConvention, RouteConfigurationContext>.Create(x => x
                .Append<DefaultRouteConvention>());

        public PluginDefinition<IInlineConstraintBuilder> ConstraintBuilder { get; } =
            PluginDefinition<IInlineConstraintBuilder>
                .Create<DefaultInlineConstraintBuilder>();

        public PluginDefinitions<IUrlConvention, UrlConfigurationContext> UrlConventions { get; } =
            PluginDefinitions<IUrlConvention, UrlConfigurationContext>.Create(x => x
                .Append<DefaultUrlConvention>()
                .Append<AliasUrlConvention>());

        public PluginDefinitions<IActionDecorator, ActionConfigurationContext> ActionDecorators { get; } =
            PluginDefinitions<IActionDecorator, ActionConfigurationContext>.Create();

        public List<Func<ActionMethod, Url, string>> UrlAliases { get; } =
            new List<Func<ActionMethod, Url, string>>();
        public string UrlPrefix { get; set; }

        public List<HttpMethod> SupportedHttpMethods { get; } = new List<HttpMethod> {
            HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete,
            HttpMethod.Options, HttpMethod.Head, HttpMethod.Trace, HttpMethod.Connect };

        public string DiagnosticsUrl { get; set; } = "_graphite";
        public bool Diagnostics { get; set; }
        public bool Metrics { get; set; } = true;
        public HttpStatusCode DefaultStatusCode = HttpStatusCode.NoContent;
        public string UnhandledExceptionStatusText { get; set; } =
            "There was a problem processing your request.";
        public bool DefaultErrorHandlerEnabled { get; set; } = true;
        public int DownloadBufferSize { get; set; } = 1024 * 1024;
        public bool AutomaticallyConstrainUrlParameterByType { get; set; }

        public string HandlerNameFilterRegex { get; set; } = "Handler$";

        public Func<Configuration, TypeDescriptor, bool> HandlerFilter { get; set; } =
            (c, t) => t.Name.IsMatch(c.HandlerNameFilterRegex);

        public Func<Configuration, string> ActionRegex { get; set; } =
            c => $"({c.SupportedHttpMethods.Select(m => m.ActionRegex).Join("|")})";

        public Func<Configuration, MethodDescriptor, bool> ActionFilter { get; set; } =
            (c, a) => a.Name.IsMatch(c.ActionRegex(c));

        public string HandlerNamespaceRegex { get; set; } = "(.*)";

        public Func<Configuration, ActionMethod, string> GetHandlerNamespace { get; set; } =
            (c, a) => a.HandlerTypeDescriptor.Type.Namespace.MatchGroups(c.HandlerNamespaceRegex).FirstOrDefault();

        public Func<Configuration, ActionMethod, string> GetActionMethodName { get; set; } =
            (c, a) => a.MethodDescriptor.Name.Remove(c.ActionRegex(c));

        public Func<Configuration, ActionMethod, string> GetHttpMethod { get; set; } = (c, a) =>
            a.MethodDescriptor.Name.MatchGroups(c.ActionRegex(c)).FirstOrDefault();

        public PluginDefinition<IBehaviorChainInvoker> BehaviorChainInvoker { get; } =
            PluginDefinition<IBehaviorChainInvoker>.Create<BehaviorChainInvoker>();

        public PluginDefinition<IActionInvoker> ActionInvoker { get; } =
            PluginDefinition<IActionInvoker>.Create<ActionInvoker>();

        public PluginDefinition<IBehavior> DefaultBehavior { get; } =
            PluginDefinition<IBehavior>.Create<InvokerBehavior>();

        public BindingMode HeadersBindingMode { get; set; } = BindingMode.None;
        public BindingMode CookiesBindingMode { get; set; } = BindingMode.None;
        public BindingMode RequestInfoBindingMode { get; set; } = BindingMode.None;
        public BindingMode ContinerBindingMode { get; set; } = BindingMode.None;
        public bool BindComplexTypeProperties { get; set; }

        public PluginDefinitions<IRequestBinder, ActionConfigurationContext> RequestBinders { get; } =
            PluginDefinitions<IRequestBinder, ActionConfigurationContext>.Create(x => x
                .Append<ReaderBinder>()
                .Append<UrlParameterBinder>()
                .Append<QuerystringBinder>()
                .Append<FormBinder>()
                .Append<JsonBinder>()
                .Append<XmlBinder>()
                .Append<HeaderBinder>()
                .Append<CookieBinder>()
                .Append<RequestInfoBinder>()
                .Append<ContainerBinder>()
            );

        public PluginDefinitions<IRequestReader, ActionConfigurationContext> RequestReaders { get; } =
            PluginDefinitions<IRequestReader, ActionConfigurationContext>.Create(x => x
                .Append<StringReader>()
                .Append<StreamReader>()
                .Append<ByteReader>()
                .Append<JsonReader>()
                .Append<XmlReader>()
                .Append<FormReader>());

        public PluginDefinitions<IValueMapper, ValueMapperConfigurationContext> ValueMappers { get; } =
            PluginDefinitions<IValueMapper, ValueMapperConfigurationContext>.Create(x => x
                .Append<SimpleTypeMapper>(singleton: true));

        public PluginDefinitions<IResponseWriter, ActionConfigurationContext> ResponseWriters { get; } =
            PluginDefinitions<IResponseWriter, ActionConfigurationContext>.Create(x => x
                .Append<RedirectWriter>()
                .Append<StringWriter>()
                .Append<StreamWriter>()
                .Append<ByteWriter>()
                .Append<JsonWriter>()
                .Append<XmlWriter>());

        public PluginDefinitions<IBehavior, ActionConfigurationContext> Behaviors { get; } =
            PluginDefinitions<IBehavior, ActionConfigurationContext>.Create(x => x
                .Append<DefaultErrorHandlerBehavior>(y => y.Configuration.DefaultErrorHandlerEnabled));
    }
}
