using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Routing
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class WildcardAttribute : Attribute { }

    public class DefaultRouteConvention : IRouteConvention
    {
        private readonly Configuration _configuration;
        private readonly IEnumerable<IUrlConvention> _urlConventions;

        public DefaultRouteConvention(Configuration configuration,
            IEnumerable<IUrlConvention> urlConventions)
        {
            _configuration = configuration;
            _urlConventions = urlConventions;
        }

        public virtual bool AppliesTo(RouteContext context)
        {
            return true;
        }

        public virtual List<RouteDescriptor> GetRouteDescriptors(RouteContext context)
        {
            var action = context.ActionMethod;
            var httpMethod = GetHttpMethod(action);
            var requestParameter = GetRequestParameter(action, httpMethod);
            var actionParameters = GetActionParameters(action, requestParameter);

            var methodSegments = _configuration.GetActionMethodName(_configuration, action).
                Split('_').Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x =>
                {
                    var urlParameter = actionParameters.FirstOrDefault(p => p.Name.EqualsIgnoreCase(x));
                    var isWildcard = urlParameter?.HasAttributes<WildcardAttribute, ParamArrayAttribute>() ?? false;
                    return new
                    {
                        Segment = urlParameter == null ? x : $"{{{(isWildcard ? "*" : "")}{urlParameter.Name}}}",
                        UrlParameter = urlParameter != null ? new UrlParameter(urlParameter, isWildcard) : null
                    };
                }).ToList();

            var urlParameters = methodSegments
                .Where(s => s.UrlParameter != null)
                .Select(x => x.UrlParameter).ToArray();

            if (urlParameters.Count(x => x.IsWildcard) > 1)
                throw new InvalidOperationException("Multiple wildcard parameters found on " +
                    $"action {action.HandlerType.FriendlyFullName}.{action.Method.FriendlyName}.");

            var parameters = actionParameters.Where(p => !urlParameters.Contains(p)).ToArray();

            var urlSegments = _configuration.GetHandlerNamespace(_configuration, action)
                .Split('.').Where(x => x.IsNotNullOrWhiteSpace())
                .Concat(methodSegments.Select(x => x.Segment)).ToArray();

            var responseBody = !action.Method.HasResult ? null : action.Method.ReturnType;

            var urlContext = new UrlContext(_configuration, context.HttpConfiguration, 
                action, httpMethod, urlSegments, urlParameters, 
                    parameters, requestParameter, responseBody);

            return _urlConventions.ThatApplyTo(urlContext, _configuration)
                .SelectMany(uc => uc.GetUrls(urlContext))
                .Select(u => new RouteDescriptor(httpMethod, u, urlParameters,
                    parameters, requestParameter, responseBody)).ToList();
        }

        private string GetHttpMethod(ActionMethod action)
        {
            return _configuration.GetHttpMethod(_configuration, action)
                .AssertNotEmptyOrWhitespace("Http method not found on " +
                    $"action {action.HandlerType.Type.FullName}.{action.Method.Name}.")
                .Trim().ToUpper();
        }

        private List<ActionParameter> GetActionParameters(ActionMethod action, 
            ParameterDescriptor requestParameter)
        {
            var actionParameters = action.Method.Parameters.Where(x => x != requestParameter)
                .Select(x => new ActionParameter(x)).ToList();

            if (_configuration.BindComplexTypeProperties)
                actionParameters.AddRange(action.Method.Parameters
                    .Where(x => x.ParameterType.IsComplexType)
                    .SelectMany(param => param.ParameterType.Properties
                        .Where(x => x.PropertyInfo.CanWrite && x.PropertyType.IsSimpleType)
                        .Select(prop => new ActionParameter(param, prop))));

            return actionParameters;
        }

        private ParameterDescriptor GetRequestParameter(ActionMethod action, string httpMethod)
        {
            return !_configuration.SupportedHttpMethods
                    .Any(x => x.AllowRequestBody && x.Method == httpMethod) ? null :
                action.Method.Parameters.Take(1)
                    .Select(x => new
                    {
                        Type = x,
                        FromUri = x.HasAttribute<FromUriAttribute>(),
                        FromBody = x.HasAttribute<FromBodyAttribute>(),
                        x.ParameterType.IsSimpleTypeOrHasSimpleElementType,
                        x.ParameterType.IsComplexTypeOrHasComplexElementType
                    })
                    .FirstOrDefault(x =>
                        (x.IsSimpleTypeOrHasSimpleElementType && x.FromBody) ||
                        (x.IsComplexTypeOrHasComplexElementType && !x.FromUri))?.Type;
        }
    }
}
