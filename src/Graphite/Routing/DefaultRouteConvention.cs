using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Graphite.Extensions;
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
            var httpMethod = _configuration.GetHttpMethod(_configuration, action)
                .AssertNotEmptyOrWhitespace("Http method not found on " +
                    $"action {action.HandlerType.Type.FullName}.{action.Method.Name}.")
                .Trim().ToUpper();

            var requestParameter = !_configuration.SupportedHttpMethods
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
                    .FirstOrDefault(x => (x.IsSimpleTypeOrHasSimpleElementType && x.FromBody) ||
                                         (x.IsComplexTypeOrHasComplexElementType && !x.FromUri))?.Type;

            var parameters = action.Method.Parameters.Where(x => x != requestParameter).ToList();

            var methodSegments = _configuration.GetActionMethodName(_configuration, action).
                Split('_').Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x =>
                {
                    var urlParameter = parameters.FirstOrDefault(p => 
                        p.Name.EqualsIgnoreCase(x));
                    var isWildcard = urlParameter?.HasAnyAttribute<WildcardAttribute, 
                        ParamArrayAttribute>() ?? false;
                    return new
                    {
                        Segment = urlParameter == null 
                            ? x : $"{{{(isWildcard ? "*" : "")}{urlParameter.Name}}}",
                        UrlParameter = urlParameter,
                        IsWildcard = isWildcard
                    };
                }).ToList();

            if (methodSegments.Count(x => x.IsWildcard) > 1)
                throw new InvalidOperationException("Multiple wildcard parameters found on " +
                    $"action {action.HandlerType.FriendlyFullName}.{action.Method.FriendlyName}.");

            var querystringParameters = parameters.Where(p =>
                !methodSegments.Any(s => s.UrlParameter == p)).ToArray();

            var urlParameters = parameters.Where(p => methodSegments
                .Any(s => s.UrlParameter == p)).ToArray();

            var wildcardParameters = parameters.Where(p => methodSegments
                .Any(s => s.IsWildcard)).ToArray();

            var urlSegments = _configuration.GetHandlerNamespace(_configuration, action)
                .Split('.').Where(x => x.IsNotNullOrWhiteSpace())
                .Concat(methodSegments.Select(x => x.Segment)).ToArray();

            var responseBody = !action.Method.HasResult ? null : action.Method.ReturnType;

            var urlContext = new UrlContext(_configuration, context.HttpConfiguration, 
                action, httpMethod, urlSegments, urlParameters, wildcardParameters, 
                    querystringParameters, requestParameter, responseBody);

            return _urlConventions.ThatApplyTo(urlContext, _configuration)
                .SelectMany(uc => uc.GetUrls(urlContext))
                .Select(u => new RouteDescriptor(httpMethod, u, urlParameters,
                    wildcardParameters, querystringParameters, requestParameter, 
                    responseBody)).ToList();
        }
    }
}
