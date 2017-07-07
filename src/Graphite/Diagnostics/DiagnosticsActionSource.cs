using System.Collections.Generic;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Reflection;
using Graphite.Routing;
using Graphite.Setup;

namespace Graphite.Diagnostics
{
    public class DiagnosticsActionSource : IActionSource
    {
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;
        private readonly ITypeCache _typeCache;
        private readonly DefaultInlineConstraintBuilder _constraintBuilder;
        private readonly ActionDescriptorFactory _actionDescriptorFactory;

        public DiagnosticsActionSource(Configuration configuration,
            HttpConfiguration httpConfiguration, ITypeCache typeCache,
            DefaultInlineConstraintBuilder constraintBuilder,
            ActionDescriptorFactory actionDescriptorFactory)
        {
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
            _typeCache = typeCache;
            _constraintBuilder = constraintBuilder;
            _actionDescriptorFactory = actionDescriptorFactory;
        }

        public bool Applies()
        {
            return _configuration.Diagnostics;
        }

        public List<ActionDescriptor> GetActions()
        {
            var configuration = new Configuration();
            var actionMethodSource = new DefaultActionMethodSource(configuration, _typeCache);
            var urlConvention = new DefaultUrlConvention(configuration, _httpConfiguration);
            var routeConvention = new DefaultRouteConvention(configuration,
                _httpConfiguration, urlConvention.AsList<IUrlConvention>(), _constraintBuilder);

            new ConfigurationDsl(configuration, _httpConfiguration)
                .IncludeTypeAssembly<DiagnosticsActionSource>()
                .OnlyIncludeHandlersUnder<DiagnosticsActionSource>()
                .ConfigureNamespaceUrlMapping(x => x.Clear()
                    .MapNamespaceAfter<DiagnosticsActionSource>())
                .WithUrlPrefix(_configuration.DiagnosticsUrl.Trim('/'))
                .ConfigureUrlConventions(x => x.Clear().Append(urlConvention))
                .ConfigureActionMethodSources(x => x.Clear().Append(actionMethodSource))
                .ConfigureRouteConventions(x => x.Clear().Append(routeConvention));

            return new DefaultActionSource(configuration, _httpConfiguration,
                actionMethodSource.AsList(), routeConvention.AsList(),
                _actionDescriptorFactory).GetActions();
        }
    }
}
