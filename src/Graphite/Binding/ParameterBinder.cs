using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Linq;
using Graphite.Routing;

namespace Graphite.Binding
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class NameAttribute : Attribute
    {
        public NameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class ParameterBinder<TStatus>
    {
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;
        private readonly IEnumerable<IValueMapper> _mappers;
        private readonly ActionMethod _actionMethod;
        private readonly RouteDescriptor _routeDescriptor;

        public ParameterBinder(Configuration configuration,
            HttpConfiguration httpConfiguration,
            ActionMethod actionMethod,
            RouteDescriptor routeDescriptor,
            IEnumerable<IValueMapper> mappers)
        {
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
            _mappers = mappers;
            _actionMethod = actionMethod;
            _routeDescriptor = routeDescriptor;
        }

        public virtual TStatus Bind(ILookup<string, object> values,
            IEnumerable<ActionParameter> actionParameters,
            Action<ActionParameter, object> bind,
            Func<TStatus> successStatus, Func<string, TStatus> failureStatus,
            Func<ActionParameter, string> mapName = null)
        {
            var parameterValues = values
                .Where(x => x.Any())
                .JoinUncase(actionParameters, x => x.Key,
                    x => mapName?.Invoke(x) ?? MapName(x),
                    (p, ap) => new
                    {
                        Parameter = ap,
                        Values = p.ToArray()
                    });

            foreach (var parameterValue in parameterValues)
            {
                var result = _mappers.Map(_actionMethod, _routeDescriptor,
                    parameterValue.Parameter, parameterValue.Values,
                    _configuration, _httpConfiguration);

                switch (result.Status)
                {
                    case MappingStatus.Failure: return failureStatus(result.ErrorMessage);
                    case MappingStatus.NoMapper:
                        if (_configuration.FailIfNoMapperFound)
                            throw new MapperNotFoundException(
                                parameterValue.Values, parameterValue.Parameter);
                        break;
                    case MappingStatus.Success:
                        bind(parameterValue.Parameter, result.Value);
                        break;
                }
            }
            return successStatus();
        }

        protected virtual string MapName(ActionParameter parameter)
        {
            return parameter.GetAttribute<NameAttribute>()?.Name ?? parameter.Name;
        }
    }
}