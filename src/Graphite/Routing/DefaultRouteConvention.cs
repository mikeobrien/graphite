using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Routing
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class WildcardAttribute : Attribute { }

    public class DefaultRouteConvention : IRouteConvention
    {
        private readonly Configuration _configuration;
        private readonly IEnumerable<IUrlConvention> _urlConventions;
        private readonly IInlineConstraintBuilder _constraintBuilder;
        private readonly ConfigurationContext _configurationContext;

        public DefaultRouteConvention(
            ConfigurationContext configurationContext,
            IEnumerable<IUrlConvention> urlConventions,
            IInlineConstraintBuilder constraintBuilder)
        {
            _configuration = configurationContext.Configuration;
            _urlConventions = urlConventions;
            _constraintBuilder = constraintBuilder;
            _configurationContext = configurationContext;
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

            var methodSegments = _configuration.GetActionMethodName(_configuration, action)
                .Split('_').Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x => GetSegment(x, actionParameters)).ToList();

            var urlParameters = methodSegments
                .Where(s => s.IsParameter)
                .Select(x => x.Parameter).ToArray();

            if (urlParameters.Count(x => x.IsWildcard) > 1)
                throw new InvalidOperationException("Multiple wildcard parameters found on " +
                    $"action {action.HandlerTypeDescriptor.FriendlyFullName}." +
                    $"{action.MethodDescriptor.FriendlyName}.");

            var parameters = actionParameters.Where(p => !urlParameters.Contains(p)).ToArray();

            var url = _configuration.GetHandlerNamespace(_configuration, action)
                .Split('.').Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x => new Segment(x))
                .Concat(methodSegments.Select(x => x)).ToUrl();

            var responseBody = !action.MethodDescriptor.HasResult ? null : action.MethodDescriptor.ReturnType;

            var urlContext = new UrlContext(action, httpMethod, url, 
                urlParameters, parameters, requestParameter, responseBody);

            return _urlConventions.ThatApplyTo(urlContext, _configurationContext)
                .SelectMany(uc => uc.GetUrls(urlContext))
                .Select(u => new RouteDescriptor(httpMethod, u, urlParameters,
                    parameters, requestParameter, responseBody)).ToList();
        }

        private string GetHttpMethod(ActionMethod action)
        {
            return _configuration.GetHttpMethod(_configuration, action)
                .AssertNotEmptyOrWhitespace("Http method not found on " +
                    $"action {action.HandlerTypeDescriptor.Type.FullName}.{action.MethodDescriptor.Name}.")
                .Trim().ToUpper();
        }

        private Segment GetSegment(string segment, List<ActionParameter> actionParameters)
        {
            var parameter = actionParameters.FirstOrDefault(p => p.Name.EqualsIgnoreCase(segment));
            if (parameter == null) return new Segment(segment);

            var isWildcard = parameter.HasAttributes<WildcardAttribute, ParamArrayAttribute>();
            var urlParameter = new UrlParameter(parameter, isWildcard);
            return new Segment(urlParameter, _constraintBuilder.Build(urlParameter));
        }

        private List<ActionParameter> GetActionParameters(ActionMethod action, 
            ParameterDescriptor requestParameter)
        {
            var actionParameters = action.MethodDescriptor.Parameters
                .Where(x => x != requestParameter)
                .Select(x => new ActionParameter(x)).ToList();

            if (_configuration.BindComplexTypeProperties)
                actionParameters.AddRange(action.MethodDescriptor.Parameters
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
                action.MethodDescriptor.Parameters
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
