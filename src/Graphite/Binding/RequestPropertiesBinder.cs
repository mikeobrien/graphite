using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Http;
using Graphite.Routing;
using Graphite.Extensions;
using Graphite.Linq;

namespace Graphite.Binding
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class FromRequestPropertiesAttribute : Attribute
    {
        public FromRequestPropertiesAttribute() { }

        public FromRequestPropertiesAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class RequestPropertiesBinder : IRequestBinder
    {
        private readonly ParameterBinder _parameterBinder;
        private readonly IRequestPropertiesProvider _requestProperties;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly Configuration _configuration;

        public RequestPropertiesBinder(Configuration configuration,
            RouteDescriptor routeDescriptor, 
            ParameterBinder parameterBinder,
            IRequestPropertiesProvider requestProperties)
        {
            _parameterBinder = parameterBinder;
            _requestProperties = requestProperties;
            _routeDescriptor = routeDescriptor;
            _configuration = configuration;
        }

        public bool AppliesTo(RequestBinderContext context)
        {
            return _routeDescriptor.Parameters.Any() &&
                   _configuration.RequestInfoBindingMode != BindingMode.None;
        }

        public Task Bind(RequestBinderContext context)
        {
            var parameters = _routeDescriptor.Parameters
                .Where(x => IncludeParameter(x, _configuration.RequestInfoBindingMode)).ToArray();
            _parameterBinder.Bind(_requestProperties.GetProperties().ToLookup(), 
                context.ActionArguments, parameters, MapParameterName);
            return Task.CompletedTask;
        }

        private string MapParameterName(ActionParameter parameter)
        {
            return parameter.GetAttribute<FromRequestPropertiesAttribute>()?.Name ?? parameter.Name;
        }

        private bool IncludeParameter(ActionParameter parameter, BindingMode bindingMode)
        {
            return !parameter.HasAttributes<FromUriAttribute, FromBodyAttribute>() &&
                (bindingMode == BindingMode.Implicit ||
                (bindingMode == BindingMode.Explicit && parameter
                    .HasAttribute<FromRequestPropertiesAttribute>()));
        }
    }
}