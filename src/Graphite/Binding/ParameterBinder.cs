using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class ParameterBinder
    {
        private readonly ConfigurationContext _configurationContext;
        private readonly IEnumerable<IValueMapper> _mappers;
        private readonly ActionMethod _actionMethod;
        private readonly RouteDescriptor _routeDescriptor;

        public ParameterBinder(ConfigurationContext configurationContext, 
            ActionMethod actionMethod, RouteDescriptor routeDescriptor,
            IEnumerable<IValueMapper> mappers)
        {
            _configurationContext = configurationContext;
            _mappers = mappers;
            _actionMethod = actionMethod;
            _routeDescriptor = routeDescriptor;
        }

        public void Bind(ILookup<string, object> values, object[] actionArguments, 
            IEnumerable<ActionParameter> parameters, Func<ActionParameter, string> mapName = null)
        {
            if (values == null || !values.Any()) return;

            var actionParameters = parameters.Where(x => x.IsParameter || x.IsPropertyOfParameter);

            values.Where(x => x.Any())
                .JoinIgnoreCase(actionParameters, x => x.Key,
                    x => mapName?.Invoke(x) ?? x.Name,
                    (p, ap) => new
                    {
                        ActionParameter = ap,
                        Values = p.ToArray()
                    })
                .ForEach(x =>
                {
                    var result = _mappers.Map(_actionMethod, _routeDescriptor, 
                        x.ActionParameter, x.Values, _configurationContext);

                    if (!result.Mapped) return;

                    x.ActionParameter.BindArgument(actionArguments, result.Value);
                });
        }
    }
}