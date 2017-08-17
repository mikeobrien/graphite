using System;
using Graphite.Actions;
using Graphite.Authentication;
using Graphite.Extensibility;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Configures authenticators.
        /// </summary>
        public ConfigurationDsl ConfigureAuthenticators(Action<ConditionalPluginsDsl
            <IAuthenticator, ActionConfigurationContext>> configure)
        {
            _configuration.Authenticators.Configure(configure);
            return this;
        }

        /// <summary>
        /// Sets the default authentication realm.
        /// </summary>
        public ConfigurationDsl WithDefaultAuthenticationRealm(string realm)
        {
            _configuration.DefaultAuthenticationRealm = realm;
            return this;
        }

        /// <summary>
        /// Sets the default unauthorized status message.
        /// </summary>
        public ConfigurationDsl WithDefaultUnauthorizedStatusMessage(string statusMessage)
        {
            _configuration.DefaultUnauthorizedStatusMessage = statusMessage;
            return this;
        }

        /// <summary>
        /// Allows actions with an authentication behavior but no authenticators that apply.
        /// </summary>
        public ConfigurationDsl AllowSecureActionsWithNoAuthenticators()
        {
            _configuration.FailIfNoAuthenticatorsApplyToAction = false;
            return this;
        }
    }
}
