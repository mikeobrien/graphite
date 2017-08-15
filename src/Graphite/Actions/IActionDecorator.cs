using System;
using Graphite.Authentication;
using Graphite.Behaviors;
using Graphite.Binding;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
using Graphite.Readers;
using Graphite.Writers;

namespace Graphite.Actions
{
    public interface IActionDecorator : IConditional<ActionDecoratorContext>
    {
        void Decorate(ActionDecoratorContext context);
    }

    public class ActionDecoratorContext
    {
        public ActionDecoratorContext(ActionDescriptor actionDescriptor)
        {
            ActionDescriptor = actionDescriptor;
        }
        
        public virtual ActionDescriptor ActionDescriptor { get; }

        public ActionDecoratorContext ConfigureRegistry(Action<Registry> configure)
        {
            configure(ActionDescriptor.Registry);
            return this;
        }

        public ActionDecoratorContext ConfigureAuthenticators(
            Action<PluginsDsl<IAuthenticator>> configure)
        {
            ActionDescriptor.Authenticators.Configure(configure);
            return this;
        }

        public ActionDecoratorContext ConfigureRequestBinders(
            Action<PluginsDsl<IRequestBinder>> configure)
        {
            ActionDescriptor.RequestBinders.Configure(configure);
            return this;
        }

        public ActionDecoratorContext ConfigureRequestReaders(
            Action<PluginsDsl<IRequestReader>> configure)
        {
            ActionDescriptor.RequestReaders.Configure(configure);
            return this;
        }

        public ActionDecoratorContext ConfigureResponseWriters(
            Action<PluginsDsl<IResponseWriter>> configure)
        {
            ActionDescriptor.ResponseWriters.Configure(configure);
            return this;
        }

        public ActionDecoratorContext ConfigureBehaviors(
            Action<PluginsDsl<IBehavior>> configure)
        {
            ActionDescriptor.Behaviors.Configure(configure);
            return this;
        }
    }
}
