using System.Web.Http;
using Graphite.Reflection;
using Graphite.Routing;

namespace Graphite.Actions
{
    public class ActionDescriptorFactory
    {
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;
        private readonly ITypeCache _typeCache;

        public ActionDescriptorFactory(Configuration configuration,
            HttpConfiguration httpConfiguration, ITypeCache typeCache)
        {
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
            _typeCache = typeCache;
        }

        public ActionDescriptor CreateDescriptor(ActionMethod actionMethod, 
            RouteDescriptor routeDescriptor)
        {
            var actionConfigurationContext = new ActionConfigurationContext(
                _configuration, _httpConfiguration, actionMethod, routeDescriptor);
            return new ActionDescriptor(actionMethod, routeDescriptor,
                _configuration.Authenticators.CloneAllThatApplyTo(actionConfigurationContext),
                _configuration.RequestBinders.CloneAllThatApplyTo(actionConfigurationContext),
                _configuration.RequestReaders.CloneAllThatApplyTo(actionConfigurationContext),
                _configuration.ResponseWriters.CloneAllThatApplyTo(actionConfigurationContext),
                _configuration.ResponseStatus.CloneAllThatApplyTo(actionConfigurationContext),
                _configuration.Behaviors.CloneAllThatApplyTo(actionConfigurationContext), _typeCache);
        }
    }
}