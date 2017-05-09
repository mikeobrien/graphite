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
    public class FromCookiesAttribute : Attribute
    {
        public FromCookiesAttribute() { }

        public FromCookiesAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class CookieBinder : IRequestBinder
    {
        public const string CookiePostfix = "Cookie";

        private readonly ParameterBinder _parameterBinder;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly Configuration _configuration;
        private readonly HttpRequestMessage _requestMessage;

        public CookieBinder(Configuration configuration,
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
                _configuration.CookiesBindingMode != BindingMode.None;
        }

        public Task Bind(RequestBinderContext context)
        {
            var values = _requestMessage.Headers.GetCookies().ToLookup();
            var parameters = _routeDescriptor.Parameters.Where(x => 
                IncludeParameter(x, _configuration.CookiesBindingMode)).ToArray();
            _parameterBinder.Bind(values, context.ActionArguments, parameters, MapParameterName);
            return Task.CompletedTask;
        }

        private bool IncludeParameter(ActionParameter parameter, BindingMode bindingMode)
        {
            return !parameter.HasAttributes<FromUriAttribute, FromBodyAttribute>() &&
                   (bindingMode == BindingMode.Implicit ||
                    (bindingMode == BindingMode.Convention && HasCookiePostfix(parameter.Name)) ||
                    (bindingMode == BindingMode.Explicit && parameter.HasAttribute<FromCookiesAttribute>()));
        }

        private string MapParameterName(ActionParameter parameter)
        {
            var bindingMode = _configuration.CookiesBindingMode;
            if (bindingMode == BindingMode.Convention && HasCookiePostfix(parameter.Name))
            {
                return parameter.Name.Truncate(CookiePostfix.Length);
            }
            if (bindingMode == BindingMode.Explicit || bindingMode == BindingMode.Implicit)
            {
                return parameter.GetAttribute<FromCookiesAttribute>()?.Name ?? parameter.Name;
            }
            return parameter.Name;
        }

        private static bool HasCookiePostfix(string name)
        {
            return name.Length > CookiePostfix.Length && name.EndsWith(CookiePostfix);
        }
    }
}