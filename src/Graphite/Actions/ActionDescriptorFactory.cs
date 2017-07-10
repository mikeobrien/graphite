using Graphite.Routing;

namespace Graphite.Actions
{
    public class ActionDescriptorFactory
    {
        private readonly Configuration _configuration;
        private readonly ConfigurationContext _configurationContext;

        public ActionDescriptorFactory(Configuration configuration,
            ConfigurationContext configurationContext)
        {
            _configuration = configuration;
            _configurationContext = configurationContext;
        }

        public ActionDescriptor CreateDescriptor(ActionMethod actionMethod, 
            RouteDescriptor routeDescriptor)
        {
            var actionConfigurationContext = new ActionConfigurationContext(
                _configurationContext, actionMethod, routeDescriptor);
            return new ActionDescriptor(actionMethod, routeDescriptor,
                _configuration.Authenticators.CloneAllThatApplyTo(actionConfigurationContext),
                _configuration.RequestBinders.CloneAllThatApplyTo(actionConfigurationContext),
                _configuration.RequestReaders.CloneAllThatApplyTo(actionConfigurationContext),
                _configuration.ResponseWriters.CloneAllThatApplyTo(actionConfigurationContext),
                _configuration.Behaviors.CloneAllThatApplyTo(actionConfigurationContext));
        }
    }
}