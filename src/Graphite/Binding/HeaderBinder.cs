using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Binding
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class FromHeadersAttribute : Attribute
    {
        public FromHeadersAttribute() { }

        public FromHeadersAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class HeaderBinder : IRequestBinder
    {
        public const string HeaderPostfix = "Header";

        private readonly ParameterBinder _parameterBinder;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly Configuration _configuration;
        private readonly HttpRequestMessage _requestMessage;

        public HeaderBinder(Configuration configuration, 
            RouteDescriptor routeDescriptor, 
            ParameterBinder parameterBinder, 
            HttpRequestMessage requestMessage)
        {
            _parameterBinder = parameterBinder;
            _routeDescriptor = routeDescriptor;
            _configuration = configuration;
            _requestMessage = requestMessage;
        }

        public bool AppliesTo(RequestBinderContext context)
        {
            return _routeDescriptor.Parameters.Any() &&
                   _configuration.HeadersBindingMode != BindingMode.None;
        }

        public Task Bind(RequestBinderContext context)
        {
            var values = _requestMessage.Headers.ToLookup();
            var parameters = _routeDescriptor.Parameters
                .Where(x => IncludeParameter(x, _configuration.HeadersBindingMode)).ToArray();
            _parameterBinder.Bind(values, context.ActionArguments, parameters, MapParameterName);
            return Task.CompletedTask;
        }

        private string MapParameterName(ActionParameter parameter)
        {
            var bindingMode = _configuration.HeadersBindingMode;
            if (bindingMode == BindingMode.Convention && HasHeaderPostfix(parameter.Name))
            {
                return parameter.Name.Truncate(HeaderPostfix.Length);
            }
            if (bindingMode == BindingMode.Explicit || bindingMode == BindingMode.Implicit)
            {
                return parameter.GetAttribute<FromHeadersAttribute>()?.Name ?? parameter.Name;
            }
            return parameter.Name;
        }

        private bool IncludeParameter(ActionParameter parameter, BindingMode bindingMode)
        {
            return !parameter.HasAttributes<FromUriAttribute, FromBodyAttribute>() &&
                   (bindingMode == BindingMode.Implicit ||
                    (bindingMode == BindingMode.Convention && HasHeaderPostfix(parameter.Name)) ||
                    (bindingMode == BindingMode.Explicit && parameter.HasAttribute<FromHeadersAttribute>()));
        }

        private static bool HasHeaderPostfix(string name)
        {
            return name.Length > HeaderPostfix.Length && name.EndsWith(HeaderPostfix);
        }
    }
}