using System;
using System.Diagnostics;
using System.Web.Http.Routing;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Configures http route decorators.
        /// </summary>
        public ConfigurationDsl ConfigureHttpRouteDecorators(Action<ConditionalPluginsDsl
            <IHttpRouteDecorator, ActionConfigurationContext>> configure)
        {
            _configuration.HttpRouteDecorators.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures route conventions.
        /// </summary>
        public ConfigurationDsl ConfigureRouteConventions(Action<ConditionalPluginsDsl
            <IRouteConvention, RouteConfigurationContext>> configure)
        {
            _configuration.RouteConventions.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures url conventions.
        /// </summary>
        public ConfigurationDsl ConfigureUrlConventions(Action<ConditionalPluginsDsl
            <IUrlConvention, UrlConfigurationContext>> configure)
        {
            _configuration.UrlConventions.Configure(configure);
            return this;
        }

        /// <summary>
        /// Adds url aliases.
        /// </summary>
        public ConfigurationDsl WithUrlAlias(params Func<ActionMethod, Url, string>[] aliases)
        {
            _configuration.UrlAliases.AddRange(aliases);
            return this;
        }

        /// <summary>
        /// Adds a prefix to all urls.
        /// </summary>
        public ConfigurationDsl WithUrlPrefix(string prefix)
        {
            _configuration.UrlPrefix = prefix;
            return this;
        }

        /// <summary>
        /// Specifies the regex used to parse the handler namespace. The namespace is 
        /// pulled from the first capture group by default e.g. "MyApp\.Handlers\.(.*)".
        /// </summary>
        public ConfigurationDsl WithHandlerNamespaceRegex(string regex)
        {
            _configuration.HandlerNamespaceRegex = regex;
            return this;
        }

        /// <summary>
        /// Removes the types namespace from the url.
        /// </summary>
        public ConfigurationDsl ExcludeTypeNamespaceFromUrl<T>()
        {
            return ExcludeTypeNamespaceFromUrl(typeof(T));
        }

        /// <summary>
        /// Removes the calling method's type namespace from the url.
        /// </summary>
        public ConfigurationDsl ExcludeCurrentNamespaceFromUrl()
        {
            return ExcludeTypeNamespaceFromUrl(new StackFrame(1).GetMethod().ReflectedType);
        }

        /// <summary>
        /// Removes the types namespace from the url.
        /// </summary>
        public ConfigurationDsl ExcludeTypeNamespaceFromUrl(Type type)
        {
            return ExcludeNamespaceFromUrl(type.Namespace);
        }

        /// <summary>
        /// Removes the namespace from the begining of the url.
        /// </summary>
        public ConfigurationDsl ExcludeNamespaceFromUrl(string @namespace)
        {
            _configuration.HandlerNamespaceRegex = $"{@namespace.RegexEscape()}\\.?(.*)";
            return this;
        }

        /// <summary>
        /// Gets the portion of the action method name used for routing.
        /// </summary>
        public ConfigurationDsl GetActionMethodNameWith(Func<Configuration, ActionMethod, string> getName)
        {
            _configuration.GetActionMethodName = getName;
            return this;
        }

        /// <summary>
        /// Gets the http method from the action method name.
        /// </summary>
        public ConfigurationDsl GetHttpMethodWith(Func<Configuration, ActionMethod, string> getMethod)
        {
            _configuration.GetHttpMethod = getMethod;
            return this;
        }

        /// <summary>
        /// Specifies the regex used to identify handlers e.g. "Handler$".
        /// </summary>
        public ConfigurationDsl WithHandlerNameRegex(string regex)
        {
            _configuration.HandlerNameFilterRegex = regex;
            return this;
        }

        /// <summary>
        /// Specifies the regex used to identify actions. The http method is 
        /// pulled from the first capture group by default e.g. "^(Get|Post|...)".
        /// </summary>
        public ConfigurationDsl WithActionRegex(Func<Configuration, string> regex)
        {
            _configuration.ActionRegex = regex;
            return this;
        }

        /// <summary>
        /// Specifies the route mapper to use.
        /// </summary>
        public ConfigurationDsl WithRouteMapper<T>() where T : IHttpRouteMapper
        {
            _configuration.HttpRouteMapper.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the route mapper to use.
        /// </summary>
        public ConfigurationDsl WithRouteMapper<T>(T instance) where T : IHttpRouteMapper
        {
            _configuration.HttpRouteMapper.Set(instance);
            return this;
        }

        /// <summary>
        /// Automatically constrain url parameters by type.
        /// </summary>
        public ConfigurationDsl AutomaticallyConstrainUrlParameterByType()
        {
            _configuration.AutomaticallyConstrainUrlParameterByType = true;
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint resolver to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintResolver<T>() where T : IInlineConstraintResolver
        {
            _configuration.InlineConstraintResolver.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint resolver to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintResolver<T>(T instance) where T : IInlineConstraintResolver
        {
            _configuration.InlineConstraintResolver.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint builder to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintBuilder<T>() where T : IInlineConstraintBuilder
        {
            _configuration.InlineConstraintBuilder.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint builder to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintBuilder<T>(T instance) where T : IInlineConstraintBuilder
        {
            _configuration.InlineConstraintBuilder.Set(instance);
            return this;
        }
    }
}
