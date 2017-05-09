using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Reflection;

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
            ParameterDescriptor parameter, RequestContext requestContext, 
            Configuration configuration)
        {
            return configuration.ValueMappers.ThatApplyTo(writers, new
                ValueMapperContext(configuration, requestContext, parameter, values))
                .FirstOrDefault();
        }

        public static MapResult Map(this IEnumerable<IValueMapper> mappers,
            object[] values, ParameterDescriptor parameter, RequestContext requestContext,
            Configuration configuration)
        {
            var mapperContext = new ValueMapperContext(configuration, requestContext, parameter, values);
            var mapper = configuration.ValueMappers.ThatApplyTo(
                mappers, mapperContext).FirstOrDefault();
            return mapper == null ? MapResult.NotMapped() : MapResult.WasMapped(mapper.Map(mapperContext));
        }

        public static MapResult Map(this IEnumerable<IValueMapper> mappers,
            object[] values, PropertyDescriptor property, RequestContext requestContext,
            Configuration configuration)
        {
            var mapperContext = new ValueMapperContext(configuration, requestContext, property, values);
            var mapper = configuration.ValueMappers.ThatApplyTo(
                mappers, mapperContext).FirstOrDefault();
            return mapper == null ? MapResult.NotMapped() : MapResult.WasMapped(mapper.Map(mapperContext));
        }
    }
}