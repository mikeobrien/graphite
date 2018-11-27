﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Routing
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class WildcardAttribute : Attribute { }

    public class DefaultRouteConvention : IRouteConvention
    {
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;
        private readonly List<IUrlConvention> _urlConventions;
        private readonly IInlineConstraintBuilder _constraintBuilder;

        public DefaultRouteConvention(
            Configuration configuration, 
            HttpConfiguration httpConfiguration,
            List<IUrlConvention> urlConventions,
            IInlineConstraintBuilder constraintBuilder)
        {
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
            _urlConventions = urlConventions;
            _constraintBuilder = constraintBuilder;
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
            var actionParameters = GetActionParameters(action, requestParameter).ToList();
            var methodSegments = GetMethodSegments(action, actionParameters).ToList();
            var urlParameters = GetUrlParameters(methodSegments).ToList();

            ValidateUrlParameters(action, urlParameters);
            
            var parameters = GetParameters(actionParameters, urlParameters).ToList();
            var responseBody = GetResponseBody(action);

            var urlContext = new UrlContext(action, httpMethod, methodSegments, 
                urlParameters, parameters, requestParameter, responseBody);

            return _urlConventions.ThatApplyTo(urlContext, _configuration, _httpConfiguration)
                .SelectMany(uc => uc.GetUrls(urlContext))
                .Select(u => new RouteDescriptor(httpMethod, u, urlParameters,
                    parameters, requestParameter, responseBody)).ToList();
        }

        protected virtual string GetHttpMethod(ActionMethod action)
        {
            var actionMethod = action.MethodDescriptor.Name
                .MatchGroupValue((_configuration.ActionNameConvention ?? 
                    DefaultActionMethodSource.DefaultActionNameConvention)(_configuration), 
                    DefaultActionMethodSource.HttpMethodGroupName);
            var method = _configuration.SupportedHttpMethods[actionMethod]?.Method;

            return method.AssertNotEmptyOrWhitespace("Http method not found on " +
                    $"action {action.HandlerTypeDescriptor.Type.FullName}.{action.MethodDescriptor.Name}.")
                .Trim().ToUpper();
        }

        protected virtual ParameterDescriptor GetRequestParameter(ActionMethod action, string httpMethod)
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

        protected virtual IEnumerable<ActionParameter> GetActionParameters(
            ActionMethod action, ParameterDescriptor requestParameter)
        {
            var actionParameters = action.MethodDescriptor.Parameters
                .Where(x => x != requestParameter)
                .Select(x => new ActionParameter(action, x, 
                    x.GetAttribute<NameAttribute>()?.Name ?? 
                    x.GetAttribute<FromUriAttribute>()?.Name)).ToList();

            if (_configuration.BindComplexTypeProperties)
                actionParameters.AddRange(action.MethodDescriptor.Parameters
                    .Where(x => x.ParameterType.IsComplexType)
                    .SelectMany(param => param.ParameterType.Properties
                        .Where(x => x.PropertyInfo.CanWrite)
                        .Select(prop => new ActionParameter(action, param, prop, 
                            prop.GetAttribute<NameAttribute>()?.Name ?? 
                            prop.GetAttribute<FromUriAttribute>()?.Name))));

            return actionParameters;
        }

        protected virtual IEnumerable<UrlSegment> GetMethodSegments(ActionMethod action, 
            List<ActionParameter> actionParameters)
        {
            var segments = (_configuration.ActionSegmentsConvention ?? 
                DefaultActionSegmentsConvention)(_configuration, action);
            if (segments == null || !segments.Any()) return Enumerable.Empty<UrlSegment>();
            return segments
                .Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x => GetSegment(x, actionParameters));
        }

        public static string[] DefaultActionSegmentsConvention(Configuration configuration, ActionMethod action)
        {
            var segments = action.MethodDescriptor.Name.MatchGroupValue(
                (configuration.ActionNameConvention ?? DefaultActionMethodSource
                       .DefaultActionNameConvention)(configuration), 
                    DefaultActionMethodSource.ActionSegmentsGroupName);
            return segments.IsNotNullOrEmpty()
                ? segments.Split("_", StringSplitOptions.RemoveEmptyEntries)
                : null;
        }

        protected virtual UrlSegment GetSegment(string segment, List<ActionParameter> actionParameters)
        {
            var parameter = actionParameters.FirstOrDefault(p =>
                p.Name.EqualsUncase(segment));
            if (parameter == null) return new UrlSegment(segment);

            var isWildcard = parameter.HasAttributes<WildcardAttribute, ParamArrayAttribute>();
            var urlParameter = new UrlParameter(parameter, isWildcard);
            return new UrlSegment(urlParameter, _constraintBuilder.Build(urlParameter));
        }

        protected virtual IEnumerable<UrlParameter> GetUrlParameters(List<UrlSegment> methodSegments)
        {
            return methodSegments
                .Where(s => s.IsParameter)
                .Select(x => x.Parameter);
        }

        protected virtual void ValidateUrlParameters(ActionMethod action, 
            List<UrlParameter> urlParameters)
        {
            if (urlParameters.Count(x => x.IsWildcard) > 1)
                throw new InvalidOperationException("Multiple wildcard parameters found on " +
                    $"action {action.HandlerTypeDescriptor.FriendlyFullName}." +
                    $"{action.MethodDescriptor.FriendlyName}.");
        }

        protected virtual IEnumerable<ActionParameter> GetParameters(
            List<ActionParameter> actionParameters, List<UrlParameter> urlParameters)
        {
            return actionParameters.Where(p => !urlParameters.Contains(p));
        }

        protected virtual TypeDescriptor GetResponseBody(ActionMethod action)
        {
            return !action.MethodDescriptor.HasResult ? null : action.MethodDescriptor.ReturnType;
        }
    }
}
