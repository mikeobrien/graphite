using System.Collections.Generic;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Reflection;
using Graphite.Routing;

namespace Graphite.Diagnostics
{
    public class DiagnosticsActionSource : IActionSource
    {
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;
        private readonly ITypeCache _typeCache;
        private readonly DefaultInlineConstraintBuilder _constraintBuilder;

        public DiagnosticsActionSource(Configuration configuration, 
            HttpConfiguration httpConfiguration, ITypeCache typeCache,
            DefaultInlineConstraintBuilder constraintBuilder)
        {
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
            _typeCache = typeCache;
            _constraintBuilder = constraintBuilder;
        }

        public bool Applies()
        {
            return _configuration.Diagnostics;
        }

        public List<ActionDescriptor> GetActions()
        {
            var configuration = new Configuration();
            var configurationContext = new ConfigurationContext(configuration, _httpConfiguration);

            new ConfigurationDsl(configuration, _httpConfiguration)
                .IncludeTypeAssembly<DiagnosticsActionSource>()
                .OnlyIncludeHandlersUnder<DiagnosticsActionSource>()
                .ExcludeTypeNamespaceFromUrl<DiagnosticsActionSource>();

            var actionMethodSource = new DefaultActionMethodSource(configuration, _typeCache);
            var urlConvention = new LambdaUrlConvention((a, s) => _configuration
                .DiagnosticsUrl.Trim('/').JoinUrls(s.ToString()).AsArray());
            var urlConventions = new List<IUrlConvention> { urlConvention };
            var routeConvention = new DefaultRouteConvention(
                configurationContext, urlConventions, _constraintBuilder);
            return new DefaultActionSource(configurationContext, actionMethodSource.AsList(), 
                routeConvention.AsList(), _typeCache)
                .GetActions();
        }
    }
}
