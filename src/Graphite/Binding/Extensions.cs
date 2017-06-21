using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Binding
{
    public static class Extensions
    {
        public static IEnumerable<IRequestBinder> ThatApplyTo(
            this IEnumerable<IRequestBinder> binders, object[] actionArguments, 
            ActionConfigurationContext actionConfigurationContext)
        {
            return actionConfigurationContext.Configuration.RequestBinders.ThatApplyTo(binders,
                actionConfigurationContext, new RequestBinderContext(actionArguments));
        }

        public static MapResult Map(this IEnumerable<IValueMapper> mappers,
            ActionMethod actionMethod, RouteDescriptor route,
            ActionParameter parameter, object[] values, 
            ConfigurationContext configurationContext)
        {
            var mapperContext = new ValueMapperContext(parameter, values);
            var mapperConfigurationcontext = new ValueMapperConfigurationContext(
                configurationContext, actionMethod, route, parameter, values);
            var mapper = configurationContext.Configuration.ValueMappers
                .FirstThatAppliesToOrDefault(mappers, mapperConfigurationcontext, mapperContext);
            return mapper == null ? MapResult.NotMapped() : MapResult.WasMapped(mapper.Map(mapperContext));
        }

        public static void BindArgument(this ActionParameter parameter, object[] arguments, object value)
        {
            if (parameter.IsProperty)
                throw new InvalidOperationException($"{parameter.Name} " +
                    "must be a parameter or parameter property.");

            var targetParameter = arguments.GetItem(
                parameter.ParameterDescriptor.Position);

            if (parameter.IsParameter) targetParameter.Value = value;
            else
            {
                if (targetParameter.Value == null) targetParameter.Value =
                    parameter.ParameterDescriptor.ParameterType.TryCreate();

                if (targetParameter.Value != null)
                    parameter.PropertyDescriptor
                        .SetValue(targetParameter.Value, value);
            }
        }

        public static object GetArgument(this ActionParameter parameter, object[] arguments)
        {
            if (parameter.IsProperty)
                throw new InvalidOperationException($"{parameter.Name} " +
                    "must be a parameter or parameter property.");

            var argument = arguments[parameter.ParameterDescriptor.Position];

            if (parameter.IsParameter) return argument;
            return argument == null ? null : parameter
                .PropertyDescriptor.GetValue(argument);
        }

        public static void BindProperty(this ActionParameter parameter, object instance, object value)
        {
            if (parameter.IsParameter)
                throw new InvalidOperationException($"{parameter.Name} " +
                    "must be a property or parameter property.");

            parameter.PropertyDescriptor.SetValue(instance, value);
        }

        public static Task Bind(this IRequestBinder binder,
            Configuration configuration, object[] actionArguments)
        {
            return binder.Bind(new RequestBinderContext(actionArguments));
        }
    }
}