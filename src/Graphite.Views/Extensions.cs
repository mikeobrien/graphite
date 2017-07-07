using System;
using System.Collections.Generic;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Setup;
using Graphite.Views.Engines;
using Graphite.Views.ViewSource;
using RazorEngine.Templating;

namespace Graphite.Views
{
    public static class Extensions
    {
        public static ConfigurationDsl EnableViews(
            this ConfigurationDsl configuration, 
            Action<ViewConfigurationDsl> configure = null)
        {
            var viewConfiguration = new ViewConfiguration();
            configure?.Invoke(new ViewConfigurationDsl(viewConfiguration));
            configuration
                .ConfigureActionDecorators(x => x
                    .Append<ViewDecorator>())
                .ConfigureRegistry(x => x
                    .Register(RazorEngineService.Create(
                        viewConfiguration.RazorConfiguration))
                    .Register(viewConfiguration)
                    .RegisterPlugins(viewConfiguration.ViewEngines)
                    .RegisterPlugins(viewConfiguration.ViewSources));

            return configuration;
        }
        
        public static IEnumerable<IViewSource> ThatApplyTo(
            this IEnumerable<IViewSource> viewSources, string[] supportedTypes,
            ActionDescriptor actionDescriptor, Configuration configuration,
            ViewConfiguration viewConfiguration, HttpConfiguration httpConfiguration)
        {
            return viewConfiguration.ViewSources.ThatAppliesTo(viewSources,
                new ActionConfigurationContext(configuration, httpConfiguration,
                    actionDescriptor.Action, actionDescriptor.Route),
                new ViewSourceContext(actionDescriptor, supportedTypes));
        }

        public static IEnumerable<IViewEngine> ThatApplyTo(
            this IEnumerable<IViewEngine> viewEngines, 
            ActionDescriptor actionDescriptor, Configuration configuration,
            ViewConfiguration viewConfiguration, HttpConfiguration httpConfiguration)
        {
            return viewConfiguration.ViewEngines.ThatAppliesTo(viewEngines,
                new ActionConfigurationContext(configuration, httpConfiguration,
                    actionDescriptor.Action, actionDescriptor.Route),
                new ViewEngineContext(actionDescriptor));
        }
    }
}
