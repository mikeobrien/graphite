using System.Collections.Generic;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Reflection;
using Graphite.Routing;

namespace Graphite.Diagnostics
{
    public class DiagnosticsActionSource : IActionSource
    {
        private readonly Configuration _configuration;
        private readonly ITypeCache _typeCache;

        public DiagnosticsActionSource(Configuration configuration, ITypeCache typeCache)
        {
            _configuration = configuration;
            _typeCache = typeCache;
        }

        public bool AppliesTo(ActionSourceContext context)
        {
            return _configuration.EnableDiagnostics;
        }

        public List<ActionDescriptor> GetActions(ActionSourceContext context)
        {
            var configuration = new Configuration();

            new ConfigurationDsl(configuration)
                .IncludeTypeAssembly<DiagnosticsActionSource>()
                .OnlyIncludeHandlersUnder<DiagnosticsActionSource>()
                .ExcludeTypeNamespaceFromUrl<DiagnosticsActionSource>();

            var actionMethodSource = new DefaultActionMethodSource(configuration, _typeCache);
            var urlConvention = new LambdaUrlConvention((a, s) => _configuration
                .DiagnosticsUrl.Trim('/').AsList(s).Join("/").AsArray());
            var urlConventions = new List<IUrlConvention> { urlConvention };
            var routeConvention = new DefaultRouteConvention(configuration, urlConventions);
            return new DefaultActionSource(actionMethodSource.AsList(), 
                routeConvention.AsList(), configuration, _typeCache)
                .GetActions(new ActionSourceContext(configuration, context.HttpConfiguration));
        }
    }
}
