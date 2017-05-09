using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Binding
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class FromRequestInfoAttribute : Attribute
    {
        public FromRequestInfoAttribute() { }

        public FromRequestInfoAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class RequestInfoBinder : IRequestBinder
    {
        public const string HttpContextKey = "MS_HttpContext";

        private readonly ParameterBinder _parameterBinder;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly Configuration _configuration;
        private readonly HttpRequestMessage _requestMessage;

        public RequestInfoBinder(Configuration configuration,
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
                   _configuration.RequestInfoBindingMode != BindingMode.None;
        }

        public Task Bind(RequestBinderContext context)
        {
            var parameters = _routeDescriptor.Parameters
                .Where(x => IncludeParameter(x, _configuration.RequestInfoBindingMode)).ToArray();
            _parameterBinder.Bind(GetValues(), context.ActionArguments, parameters, MapParameterName);
            return Task.CompletedTask;
        }

        private string MapParameterName(ActionParameter parameter)
        {
            return parameter.GetAttribute<FromRequestInfoAttribute>()?.Name ?? parameter.Name;
        }

        private bool IncludeParameter(ActionParameter parameter, BindingMode bindingMode)
        {
            return !parameter.HasAttributes<FromUriAttribute, FromBodyAttribute>() &&
                   (bindingMode == BindingMode.Implicit ||
                    (bindingMode == BindingMode.Explicit && parameter.HasAttribute<FromRequestInfoAttribute>()));
        }

        private ILookup<string, object> GetValues()
        {
            if (_requestMessage.Properties.ContainsKey(HttpContextKey))
            {
                return _requestMessage.Properties[HttpContextKey].As<HttpContextBase>()?.Request?
                    .ServerVariables.ToLookup(NormalizeServerVariableKey);
            }
            if (_requestMessage.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                var endpoint = (RemoteEndpointMessageProperty)_requestMessage
                    .Properties[RemoteEndpointMessageProperty.Name];
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "RemoteAddress", endpoint.Address },
                    { "RemotePort", endpoint.Port }
                }.ToLookup();
            }
            return null;
        }

        private static string NormalizeServerVariableKey(string key)
        {
            return key.Replace("_ADDR", "address").Replace("_", "");
        }
    }
}