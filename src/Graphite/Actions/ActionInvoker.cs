using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Routing;
using Graphite.Writers;

namespace Graphite.Actions
{
    public class ActionInvoker : IActionInvoker
    {
        private readonly Configuration _configuration;
        private readonly ActionMethod _actionMethod;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly IEnumerable<IRequestBinder> _requestBinders;
        private readonly IEnumerable<IResponseWriter> _writers;
        private readonly HttpResponseMessage _responseMessage;
        private readonly ActionConfigurationContext _actionConfigurationContext;

        public ActionInvoker(
            ActionConfigurationContext actionConfigurationContext,
            ActionMethod actionMethod,
            RouteDescriptor routeDescriptor,
            IEnumerable<IRequestBinder> requestBinders, 
            IEnumerable<IResponseWriter> writers,
            HttpResponseMessage responseMessage)
        {
            _actionConfigurationContext = actionConfigurationContext;
            _configuration = actionConfigurationContext.Configuration;
            _actionMethod = actionMethod;
            _routeDescriptor = routeDescriptor;
            _requestBinders = requestBinders;
            _writers = writers;
            _responseMessage = responseMessage;
        }

        public virtual async Task<HttpResponseMessage> Invoke(object handler)
        {
            var actionArguments = new object[_actionMethod.MethodDescriptor.Parameters.Length];

            if (actionArguments.Any())
            {
                var requestBinderContext = new RequestBinderContext(actionArguments);
                foreach (var binder in _requestBinders.ThatApplyTo(actionArguments, 
                    _actionConfigurationContext))
                {
                    await binder.Bind(requestBinderContext);
                }
            }

            var response = await _actionMethod.Invoke(handler, actionArguments);

            if (response is HttpResponseMessage) return response.As<HttpResponseMessage>();

            if (_routeDescriptor.HasResponse)
            {
                var writer = _writers.ThatAppliesTo(response, _actionConfigurationContext);
                if (writer != null) return await writer.Write(response);
            }

            _responseMessage.StatusCode = _configuration.DefaultStatusCode;
            return _responseMessage;
        }
    }
}