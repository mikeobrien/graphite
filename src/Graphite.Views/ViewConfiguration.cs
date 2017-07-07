using System;
using System.Collections.Generic;
using System.Text;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Http;
using Graphite.Reflection;
using Graphite.Views.Engines;
using Graphite.Views.ViewSource;
using RazorEngine.Configuration;

namespace Graphite.Views
{
    public class ViewConfiguration
    {
        public bool ClearOtherWriters { get; set; } = true;
        public List<string> DefaultAcceptTypes { get; set; } = new List<string> { MimeTypes.TextHtml };
        public string DefaultContentType { get; set; } = MimeTypes.TextHtml;
        public Encoding DefaultEncoding { get; set; } = Encoding.UTF8;
        public Func<ViewSourceContext, string[]> ViewNameConvention { get; set; }

        public TemplateServiceConfiguration RazorConfiguration = new TemplateServiceConfiguration();

        public ConditionalPlugins<IViewEngine, ActionConfigurationContext> ViewEngines { get; } =
            new ConditionalPlugins<IViewEngine, ActionConfigurationContext>(true)
                .Configure(x => x
                    .Append<MustacheEngine>()
                    .Append<Engines.RazorEngine>());

        public ConditionalPlugins<IViewSource, ActionConfigurationContext> ViewSources { get; } =
            new ConditionalPlugins<IViewSource, ActionConfigurationContext>(true)
                .Configure(x => x
                    .Append<FileViewSource>()
                    .Append<EmbeddedViewSource>());

        public List<NamespaceMapping> NamespacePathMappings { get; set; } =
            new List<NamespaceMapping>
            {
                NamespaceMapping.DefaultMapping
            };
    }
}