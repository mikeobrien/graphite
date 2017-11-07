using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web.Http.Routing;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Reflection;
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
            <IUrlConvention, UrlContext>> configure)
        {
            _configuration.UrlConventions.Configure(configure);
            return this;
        }

        /// <summary>
        /// Adds url aliases.
        /// </summary>
        public ConfigurationDsl WithUrlAlias(params Func<UrlContext, string>[] aliases)
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

        public class NamespaceUrlMappingDsl
        {
            private readonly Configuration _configuration;

            public NamespaceUrlMappingDsl(Configuration configuration)
            {
                _configuration = configuration;
            }

            /// <summary>
            /// Clears all namespace url mappings.
            /// </summary>
            public NamespaceUrlMappingDsl Clear()
            {
                _configuration.NamespaceUrlMappings.Clear();
                return this;
            }

            /// <summary>
            /// Adds a namespace url mapping.
            /// </summary>
            /// <param name="namespaceRegex">Regex matching the namespace to map.</param>
            /// <param name="url">Resulting url. Supports substitutions e.g. $1 or ${capturegroup}</param>
            public NamespaceUrlMappingDsl Add(string namespaceRegex, string url)
            {
                _configuration.NamespaceUrlMappings.Add(
                    new NamespaceMapping(new Regex(namespaceRegex), url));
                return this;
            }

            /// <summary>
            /// Maps the namespace starting after the calling types' namespace.
            /// </summary>
            public NamespaceUrlMappingDsl MapNamespaceAfterCallingType()
            {
                return MapNamespaceAfter(new StackFrame(1).GetMethod().ReflectedType);
            }

            /// <summary>
            /// Maps the namespace starting after the types' namespace.
            /// </summary>
            public NamespaceUrlMappingDsl MapNamespaceAfter<T>()
            {
                return MapNamespaceAfter(typeof(T));
            }

            /// <summary>
            /// Maps the namespace starting after the types' namespace.
            /// </summary>
            public NamespaceUrlMappingDsl MapNamespaceAfter(Type type)
            {
                return MapNamespaceAfter(type.Namespace);
            }

            /// <summary>
            /// Maps the namespace starting after this namespace.
            /// </summary>
            public NamespaceUrlMappingDsl MapNamespaceAfter(string @namespace)
            {
                Clear();
                _configuration.NamespaceUrlMappings.Add(
                    NamespaceMapping.MapAfterNamespace(@namespace));
                return this;
            }
        }

        /// <summary>
        /// Enables you to configure the namespace to url mappings.
        /// </summary>
        public ConfigurationDsl ConfigureNamespaceUrlMapping(Action<NamespaceUrlMappingDsl> config)
        {
            config(new NamespaceUrlMappingDsl(_configuration));
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
