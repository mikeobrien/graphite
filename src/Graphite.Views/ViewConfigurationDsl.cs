using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Reflection;
using Graphite.Views.Engines;
using Graphite.Views.ViewSource;
using RazorEngine.Configuration;

namespace Graphite.Views
{
    public class ViewConfigurationDsl
    {
        private readonly ViewConfiguration _configuration;

        public ViewConfigurationDsl(ViewConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Allow multiple writers to be defined on a view action.
        /// </summary>
        public ViewConfigurationDsl AllowMutlipleWriters()
        {
            _configuration.ClearOtherWriters = false;
            return this;
        }

        /// <summary>
        /// Specifies the default accept types views apply to.
        /// </summary>
        public ViewConfigurationDsl WithDefaultAcceptTypes(params string[] acceptTypes)
        {
            _configuration.DefaultAcceptTypes = acceptTypes.ToList();
            return this;
        }

        /// <summary>
        /// Adds a default accept type that views apply to.
        /// </summary>
        public ViewConfigurationDsl AddDefaultAcceptType(string acceptType)
        {
            _configuration.DefaultAcceptTypes.Add(acceptType);
            return this;
        }

        /// <summary>
        /// Sets the default view content type.
        /// </summary>
        public ViewConfigurationDsl WithDefaultContentType(string contentType)
        {
            _configuration.DefaultContentType = contentType;
            return this;
        }

        /// <summary>
        /// Sets the default view encoding.
        /// </summary>
        public ViewConfigurationDsl WithDefaultEncoding(Encoding encoding)
        {
            _configuration.DefaultEncoding = encoding;
            return this;
        }

        /// <summary>
        /// Specifies the convention used to determine the view name.
        /// </summary>
        public ViewConfigurationDsl WithViewNameConvention(
            Func<ViewSourceContext, string[]> convention)
        {
            _configuration.ViewNameConvention = convention;
            return this;
        }

        /// <summary>
        /// Specifies the convention used to determine the view name.
        /// </summary>
        public ViewConfigurationDsl WithViewNameConvention(
            Func<ViewSourceContext, string> convention)
        {
            _configuration.ViewNameConvention = x => new [] { convention(x) };
            return this;
        }

        /// <summary>
        /// Configure view engines.
        /// </summary>
        public ViewConfigurationDsl ConfigureViewEngines(Action<ConditionalPluginsDsl
            <IViewEngine, ActionConfigurationContext>> configure)
        {
            _configuration.ViewEngines.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configure view sources.
        /// </summary>
        public ViewConfigurationDsl ConfigureViewSources(Action<ConditionalPluginsDsl
            <IViewSource, ActionConfigurationContext>> configure)
        {
            _configuration.ViewSources.Configure(configure);
            return this;
        }

        public class NamespacePathMappingDsl
        {
            private readonly ViewConfiguration _configuration;

            public NamespacePathMappingDsl(ViewConfiguration configuration)
            {
                _configuration = configuration;
            }

            /// <summary>
            /// Clears all namespace path mappings.
            /// </summary>
            public NamespacePathMappingDsl Clear()
            {
                _configuration.NamespacePathMappings.Clear();
                return this;
            }

            /// <summary>
            /// Adds a namespace path mapping.
            /// </summary>
            /// <param name="namespaceRegex">Regex matching the namespace to map.</param>
            /// <param name="path">Resulting relative path. Supports substitutions e.g. $1 or ${capturegroup}</param>
            public NamespacePathMappingDsl Add(string namespaceRegex, string path)
            {
                _configuration.NamespacePathMappings.Add(
                    new NamespaceMapping(new Regex(namespaceRegex), path));
                return this;
            }

            /// <summary>
            /// Maps the namespace starting after the calling types' namespace.
            /// </summary>
            public NamespacePathMappingDsl MapNamespaceAfterCallingType()
            {
                return MapNamespaceAfter(new StackFrame(1).GetMethod().ReflectedType);
            }

            /// <summary>
            /// Maps the namespace starting after the types' namespace.
            /// </summary>
            public NamespacePathMappingDsl MapNamespaceAfter<T>()
            {
                return MapNamespaceAfter(typeof(T));
            }

            /// <summary>
            /// Maps the namespace starting after the types' namespace.
            /// </summary>
            public NamespacePathMappingDsl MapNamespaceAfter(Type type)
            {
                return MapNamespaceAfter(type.Namespace);
            }

            /// <summary>
            /// Maps the namespace starting after this namespace.
            /// </summary>
            public NamespacePathMappingDsl MapNamespaceAfter(string @namespace)
            {
                _configuration.NamespacePathMappings.Add(
                    NamespaceMapping.MapAfterNamespace(@namespace));
                return this;
            }
        }

        /// <summary>
        /// Enables you to configure the namespace to path mappings.
        /// </summary>
        public ViewConfigurationDsl ConfigureNamespacePathMapping(
            Action<NamespacePathMappingDsl> config)
        {
            config(new NamespacePathMappingDsl(_configuration));
            return this;
        }

        /// <summary>
        /// Configure the razor view engine.
        /// </summary>
        public ViewConfigurationDsl ConfigureRazor(
            Action<TemplateServiceConfiguration> config)
        {
            config(_configuration.RazorConfiguration);
            return this;
        }
    }
}