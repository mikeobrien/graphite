using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Linq;
using Graphite.Reflection;
using Graphite.Routing;

namespace Graphite.Actions
{
    public static class Extensions
    {
        public static HttpResponseMessage SetStatus(
            this HttpResponseMessage responseMessage,
            IEnumerable<IResponseStatus> responseStatus,
            ActionDescriptor actionDescriptor, 
            ResponseState responseState,
            string errorMessage)
        {
            var context = new ResponseStatusContext(responseMessage, responseState, errorMessage);
            actionDescriptor.ResponseStatus.ThatAppliesToOrDefault(
                responseStatus, context)?.SetStatus(context);
            return responseMessage;
        }
        
        public static HttpResponseMessage ApplyHeaders(
            this HttpResponseMessage responseMessage,
            IEnumerable<IResponseHeaders> responseHeaders,
            ActionDescriptor actionDescriptor)
        {
            var context = new ResponseHeadersContext(responseMessage);
            actionDescriptor.ResponseHeaders.ThatAppliesToOrDefault(
                responseHeaders, context)?.SetHeaders(context);
            return responseMessage;
        }

        public static IEnumerable<IActionDecorator> ThatApplyTo(
            this IEnumerable<IActionDecorator> actionDecorators,
            ActionDescriptor actionDescriptor, Configuration configuration,
            HttpConfiguration httpConfiguration)
        {
            return configuration.ActionDecorators.ThatAppliesTo(actionDecorators,
                new ActionConfigurationContext(configuration, httpConfiguration, 
                    actionDescriptor.Action, actionDescriptor.Route), 
                new ActionDecoratorContext(actionDescriptor));
        }

        public static void Decorate(this IEnumerable<IActionDecorator> 
            actionSources, ActionDescriptor actionDescriptor)
        {
            var context = new ActionDecoratorContext(actionDescriptor);
            actionSources.ForEach(x => x.Decorate(context));
        }

        public static bool IsUnderNamespace<T>(this ActionMethod actionMethod, string relativeNamespace = null)
        {
            return actionMethod.HandlerTypeDescriptor.Type.IsUnderNamespace<T>(relativeNamespace);
        }

        public static bool IsGraphiteAction(this ActionMethod actionMethod)
        {
            return actionMethod.HandlerTypeDescriptor.Type.IsUnderNamespace("Graphite");
        }

        public static bool HasParameterNamed(this RouteDescriptor routeDescriptor, string name)
        {
            return routeDescriptor.Parameters.Any(x => x.Name.EqualsUncase(name));
        }

        public static bool HasPropertyNamed(this ParameterDescriptor parameter, string name)
        {
            return parameter?.ParameterType.Properties
                .Any(x => x.Name.EqualsUncase(name)) ?? false;
        }
    }
}
