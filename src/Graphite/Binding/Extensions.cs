using System;
using System.Collections.Generic;
using System.Linq;
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
            RequestContext requestContext, Configuration configuration)
        {
            return configuration.RequestBinders.ThatApplyTo(binders, 
                new RequestBinderContext(configuration, requestContext, actionArguments));
        }

        public static IValueMapper ThatApplyTo(
            this IEnumerable<IValueMapper> writers, object[] values,
            ActionParameter parameter, RequestContext requestContext, 
            Configuration configuration)
        {
            return configuration.ValueMappers.ThatApplyTo(writers, 
                new ValueMapperContext(configuration, requestContext, parameter, values))
                .FirstOrDefault();
        }

        public static MapResult Map(this IEnumerable<IValueMapper> mappers,
            object[] values, ActionParameter parameter, RequestContext requestContext,
            Configuration configuration)
        {
            var mapperContext = new ValueMapperContext(configuration, requestContext, parameter, values);
            var mapper = configuration.ValueMappers.ThatApplyTo(mappers, mapperContext).FirstOrDefault();
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

        public static void BindProperty(this ActionParameter parameter, object instance, object value)
        {
            if (parameter.IsParameter)
                throw new InvalidOperationException($"{parameter.Name} " +
                    "must be a property or parameter property.");

            parameter.PropertyDescriptor.SetValue(instance, value);
        }
    }
}