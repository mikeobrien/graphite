using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.DependencyInjection;
using Graphite.Extensions;
using Graphite.Linq;
using Graphite.Routing;

namespace Graphite.Binding
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class FromContainerAttribute : Attribute { }

    public class ContainerBinder : IRequestBinder
    {
        private readonly RouteDescriptor _routeDescriptor;
        private readonly Configuration _configuration;
        private readonly IContainer _container;

        public ContainerBinder(
            Configuration configuration,
            RouteDescriptor routeDescriptor,
            IContainer container)
        {
            _configuration = configuration;
            _container = container;
            _routeDescriptor = routeDescriptor;
        }

        public bool AppliesTo(RequestBinderContext context)
        {
            return GetParameters().Any();
        }

        public Task Bind(RequestBinderContext context)
        {
            GetParameters().ForEach(x =>
            {
                if (x.GetArgument(context.ActionArguments) == null)
                    x.BindArgument(context.ActionArguments, _container
                        .GetInstance(x.TypeDescriptor.Type));
            });
            return Task.CompletedTask;
        }

        private IEnumerable<ActionParameter> GetParameters()
        {
            return _routeDescriptor.Parameters.Where(x => 
                x.TypeDescriptor.IsComplexType && 
                (x.IsParameter || x.IsPropertyOfParameter) &&
                (_configuration.ContinerBindingMode == BindingMode.Implicit || 
                    (_configuration.ContinerBindingMode == BindingMode.Explicit && 
                     x.HasAttribute<FromContainerAttribute>())));
        }
    }
}