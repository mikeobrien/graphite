using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
        /// Specifies the regex used to parse the handler namespace. 
        /// The namespace is pulled from the "namespace" capture 
        /// group e.g. "MyApp\.Handlers\.(?&lt;namespace&gt;.*)".
        /// </summary>
        public ConfigurationDsl WithHandlerNamespaceConvention(string regex)
        {
            _configuration.HandlerNamespaceConvention = new Regex(regex);
            return this;
        }

        /// <summary>
        /// Parses the handler namespace.
        /// </summary>
        public ConfigurationDsl WithHandlerNamespaceParser(
            Func<Configuration, ActionMethod, string> parser)
        {
            _configuration.HandlerNamespaceParser = parser;
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
            WithHandlerNamespaceConvention($"{@namespace.RegexEscape()}\\.?" + 
                Configuration.DefaultHandlerNamespaceConventionRegex);
            return this;
        }

        /// <summary>
        /// Gets action segments.
        /// </summary>
        public ConfigurationDsl WithActionSegmentsConvention(
            Func<Configuration, ActionMethod, string[]> segments)
        {
            _configuration.ActionSegmentsConvention = segments;
            return this;
        }

        /// <summary>
        /// Gets the http method of the action.
        /// </summary>
        public ConfigurationDsl WithHttpMethodConvention(Func<Configuration, 
            ActionMethod, string> method)
        {
            _configuration.HttpMethodConvention = method;
            return this;
        }

        /// <summary>
        /// Specifies the regex used to identify handlers e.g. "Handler$".
        /// </summary>
        public ConfigurationDsl WithHandlerNameConvention(string regex)
        {
            _configuration.HandlerNameConvention = new Regex(regex);
            return this;
        }

        /// <summary>
        /// Specifies the regex used to identify actions and parse action names. 
        /// By default the http method is pulled from the "method" capture group, 
        /// the segements from from the "segment" capture group e.g. 
        /// "^(?&lt;method&gt;{methods}?)(?&lt;segments&gt;.*)". 
        /// </summary>
        public ConfigurationDsl WithActionNameConvention(Func<Configuration, string> regex)
        {
            _configuration.ActionNameConvention = x => new Regex(regex(x));
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
        public ConfigurationDsl WithInlineConstraintResolver<T>() 
            where T : IInlineConstraintResolver
        {
            _configuration.InlineConstraintResolver.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint resolver to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintResolver<T>(T instance) 
            where T : IInlineConstraintResolver
        {
            _configuration.InlineConstraintResolver.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint builder to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintBuilder<T>() 
            where T : IInlineConstraintBuilder
        {
            _configuration.InlineConstraintBuilder.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the inline constraint builder to use.
        /// </summary>
        public ConfigurationDsl WithInlineConstraintBuilder<T>(T instance) 
            where T : IInlineConstraintBuilder
        {
            _configuration.InlineConstraintBuilder.Set(instance);
            return this;
        }
    }
}
