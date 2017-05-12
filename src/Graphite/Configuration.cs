using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphite.Actions;
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
            PluginDefinition<IInitializer>.Create<Initializer>();

        public PluginDefinition<ITypeCache> TypeCache { get; } =
            PluginDefinition<ITypeCache>.Create<TypeCache>();
        
        public PluginDefinitions<IActionMethodSource, ActionMethodSourceContext> ActionMethodSources { get; } =
            PluginDefinitions<IActionMethodSource, ActionMethodSourceContext>.Create(x => x
                .Append<DefaultActionMethodSource>());

        public PluginDefinitions<IActionSource, ActionSourceContext> ActionSources { get; } =
            PluginDefinitions<IActionSource, ActionSourceContext>.Create(x => x
                .Append<DiagnosticsActionSource>()
                .Append<DefaultActionSource>());

        public PluginDefinitions<IActionDecorator, ActionDecoratorContext> ActionDecorators { get; } =
            PluginDefinitions<IActionDecorator, ActionDecoratorContext>.Create();

        public PluginDefinitions<IRouteConvention, RouteContext> RouteConventions { get; } =
            PluginDefinitions<IRouteConvention, RouteContext>.Create(x => x
                .Append<DefaultRouteConvention>());

        public PluginDefinitions<IUrlConvention, UrlContext> UrlConventions { get; } =
            PluginDefinitions<IUrlConvention, UrlContext>.Create(x => x
                .Append<DefaultUrlConvention>()
                .Append<AliasUrlConvention>());

        public List<Func<ActionMethod, string[], string>> UrlAliases { get; } =
            new List<Func<ActionMethod, string[], string>>();

        public List<HttpMethod> SupportedHttpMethods { get; } = new List<HttpMethod> {
            HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete,
            HttpMethod.Options, HttpMethod.Head, HttpMethod.Trace, HttpMethod.Connect };

        public string DiagnosticsUrl { get; set; } = "_graphite";
        public bool EnableDiagnostics { get; set; }
        public bool EnableMetrics { get; set; } = true;
        public string UnhandledExceptionStatusText { get; set; } =
            "There was a problem processing your request.";
        public bool DefaultErrorHandlerEnabled { get; set; } = true;
        public int DownloadBufferSize { get; set; } = 1024 * 1024;

        public string HandlerNameFilterRegex { get; set; } = "Handler$";

        public Func<Configuration, TypeDescriptor, bool> HandlerFilter { get; set; } =
            (c, t) => t.Name.IsMatch(c.HandlerNameFilterRegex);

        public Func<Configuration, string> ActionRegex { get; set; } =
            c => $"({c.SupportedHttpMethods.Select(m => m.ActionRegex).Join("|")})";

        public Func<Configuration, MethodDescriptor, bool> ActionFilter { get; set; } =
            (c, a) => a.Name.IsMatch(c.ActionRegex(c));

        public string HandlerNamespaceRegex { get; set; } = "(.*)";

        public Func<Configuration, ActionMethod, string> GetHandlerNamespace { get; set; } =
            (c, a) => a.HandlerType.Type.Namespace.MatchGroups(c.HandlerNamespaceRegex).FirstOrDefault();

        public Func<Configuration, ActionMethod, string> GetActionMethodName { get; set; } =
            (c, a) => a.Method.Name.Remove(c.ActionRegex(c));

        public Func<Configuration, ActionMethod, string> GetHttpMethod { get; set; } = (c, a) =>
            a.Method.Name.MatchGroups(c.ActionRegex(c)).FirstOrDefault();

        public PluginDefinition<IBehaviorChainInvoker> BehaviorChainInvoker { get; } =
            PluginDefinition<IBehaviorChainInvoker>.Create<BehaviorChainInvoker>();

        public PluginDefinition<IActionInvoker> ActionInvoker { get; } =
            PluginDefinition<IActionInvoker>.Create<ActionInvoker>();

        public PluginDefinition<IInvokerBehavior> InvokerBehavior { get; } =
            PluginDefinition<IInvokerBehavior>.Create<InvokerBehavior>();

        public PluginDefinitions<IRequestBinder, RequestBinderContext> RequestBinders { get; } =
            PluginDefinitions<IRequestBinder, RequestBinderContext>.Create(x => x
                .Append<ReaderBinder>(singleton: true)
                .Append<UrlParameterBinder>(singleton: true)
                .Append<QuerystringBinder>(singleton: true)
                .Append<FormBinder>(singleton: true));

        public PluginDefinitions<IRequestReader, RequestReaderContext> RequestReaders { get; } =
            PluginDefinitions<IRequestReader, RequestReaderContext>.Create(x => x
                .Append<StringReader>(singleton: true)
                .Append<StreamReader>(singleton: true)
                .Append<JsonReader>(singleton: true)
                .Append<XmlReader>(singleton: true)
                .Append<FormReader>(singleton: true));

        public PluginDefinitions<IValueMapper, ValueMapperContext> ValueMappers { get; } =
            PluginDefinitions<IValueMapper, ValueMapperContext>.Create(x => x
                .Append<SimpleTypeMapper>(singleton: true));

        public PluginDefinitions<IResponseWriter, ResponseWriterContext> ResponseWriters { get; } =
            PluginDefinitions<IResponseWriter, ResponseWriterContext>.Create(x => x
                .Append<RedirectWriter>(singleton: true)
                .Append<StringWriter>(singleton: true)
                .Append<StreamWriter>(singleton: true)
                .Append<JsonWriter>(singleton: true)
                .Append<XmlWriter>(singleton: true));

        public PluginDefinitions<IBehavior, BehaviorContext> Behaviors { get; } =
            PluginDefinitions<IBehavior, BehaviorContext>.Create(x => x
                .Append<DefaultErrorHandlerBehavior>(y => y
                    .Configuration.DefaultErrorHandlerEnabled));
    }
}
