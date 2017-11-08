using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Binding
{
    public static class Extensions
    {
        public static MapResult Map(this IEnumerable<IValueMapper> mappers,
            ActionMethod actionMethod, RouteDescriptor route,
            ActionParameter parameter, object[] values, 
            Configuration configuration, HttpConfiguration httpConfiguration)
        {
            var mapperContext = new ValueMapperContext(parameter, values);
            var mapperConfigurationcontext = new ValueMapperConfigurationContext(
                configuration, httpConfiguration, actionMethod, route, parameter, values);
            var mapper = configuration.ValueMappers
                .FirstThatAppliesToOrDefault(mappers, mapperConfigurationcontext, mapperContext);
            return mapper == null 
                ? MapResult.NoMapper() 
                : mapper.Map(mapperContext);
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

        public static InputStream CreateInputStream(this HttpContent content)
        {
            var headers = content?.Headers;
            return new InputStream(
                headers?.ContentDisposition?.Name.Unquote(),
                headers?.ContentDisposition?.FileName.Unquote(),
                headers?.ContentType?.MediaType,
                headers?.ContentEncoding?.ToArray(),
                headers?.ContentLength,
                headers,
                content?.ReadAsStreamAsync().Result);
        }
    }
}