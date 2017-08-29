using System;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensibility;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Configures request binders.
        /// </summary>
        public ConfigurationDsl ConfigureRequestBinders(Action<ConditionalPluginsDsl
            <IRequestBinder, ActionConfigurationContext>> configure)
        {
            _configuration.RequestBinders.Configure(configure);
            return this;
        }

        /// <summary>
        /// Indicates that the request should fail if no 
        /// value mapper found for a request value.
        /// </summary>
        public ConfigurationDsl FailIfNoMapperFound()
        {
            _configuration.FailIfNoMapperFound = true;
            return this;
        }

        /// <summary>
        /// Binds header values to action parameters.
        /// </summary>
        public ConfigurationDsl BindHeaders()
        {
            _configuration.HeadersBindingMode = BindingMode.Implicit;
            return this;
        }

        /// <summary>
        /// Binds header values to action parameters by convention.
        /// </summary>
        public ConfigurationDsl BindHeadersByNamingConvention()
        {
            _configuration.HeadersBindingMode = BindingMode.Convention;
            return this;
        }

        /// <summary>
        /// Binds header values to action parameters by attribute.
        /// </summary>
        public ConfigurationDsl BindHeadersByAttribute()
        {
            _configuration.HeadersBindingMode = BindingMode.Explicit;
            return this;
        }

        /// <summary>
        /// Binds cookie values to action parameters.
        /// </summary>
        public ConfigurationDsl BindCookies()
        {
            _configuration.CookiesBindingMode = BindingMode.Implicit;
            return this;
        }

        /// <summary>
        /// Binds cookie values to action parameters by convention.
        /// </summary>
        public ConfigurationDsl BindCookiesByNamingConvention()
        {
            _configuration.CookiesBindingMode = BindingMode.Convention;
            return this;
        }

        /// <summary>
        /// Binds cookie values to action parameters by attribute.
        /// </summary>
        public ConfigurationDsl BindCookiesByAttribute()
        {
            _configuration.CookiesBindingMode = BindingMode.Explicit;
            return this;
        }

        /// <summary>
        /// Binds request info values to action parameters.
        /// </summary>
        public ConfigurationDsl BindRequestInfo()
        {
            _configuration.RequestInfoBindingMode = BindingMode.Implicit;
            return this;
        }

        /// <summary>
        /// Binds request info values to action parameters by convention.
        /// </summary>
        public ConfigurationDsl BindRequestInfoByAttribute()
        {
            _configuration.RequestInfoBindingMode = BindingMode.Explicit;
            return this;
        }

        /// <summary>
        /// Binds container values to action parameters.
        /// </summary>
        public ConfigurationDsl BindContainer()
        {
            _configuration.ContinerBindingMode = BindingMode.Implicit;
            return this;
        }

        /// <summary>
        /// Binds container values to action parameters by attribute.
        /// </summary>
        public ConfigurationDsl BindContainerByAttribute()
        {
            _configuration.ContinerBindingMode = BindingMode.Explicit;
            return this;
        }

        /// <summary>
        /// Binds request parameters to the first level of 
        /// properties of a complex action parameter type.
        /// </summary>
        public ConfigurationDsl BindComplexTypeProperties()
        {
            _configuration.BindComplexTypeProperties = true;
            return this;
        }
    }
}
