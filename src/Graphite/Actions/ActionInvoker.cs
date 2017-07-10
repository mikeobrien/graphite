using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Routing;
using Graphite.Writers;

namespace Graphite.Actions
{
    public class ActionInvoker : IActionInvoker
    {
        private readonly Configuration _configuration;
        private readonly IEnumerable<IRequestBinder> _requestBinders;
        private readonly IEnumerable<IResponseWriter> _writers;
        private readonly HttpResponseMessage _responseMessage;
        private readonly ActionDescriptor _actionDescriptor;

        public ActionInvoker(
            Configuration configuration,
            ActionDescriptor actionDescriptor,
            IEnumerable<IRequestBinder> requestBinders, 
            IEnumerable<IResponseWriter> writers,
            HttpResponseMessage responseMessage)
        {
            _actionDescriptor = actionDescriptor;
            _configuration = configuration;
            _requestBinders = requestBinders;
            _writers = writers;
            _responseMessage = responseMessage;
        }

        public virtual async Task<HttpResponseMessage> Invoke(object handler)
        {
            var actionArguments = new object[_actionDescriptor
                .Action.MethodDescriptor.Parameters.Length];

            if (actionArguments.Any())
            {
                var requestBinderContext = new RequestBinderContext(actionArguments);
                foreach (var binder in _actionDescriptor.RequestBinders
                    .ThatApplyTo(_requestBinders, requestBinderContext))
                {
                    await binder.Bind(requestBinderContext);
                }
            }

            var response = await _actionDescriptor.Action.Invoke(handler, actionArguments);

            if (response is HttpResponseMessage) return response.As<HttpResponseMessage>();

            if (_actionDescriptor.Route.HasResponse)
            {
                var writer = _writers.ThatApply(response, _actionDescriptor).FirstOrDefault();
                if (writer != null) return await writer.Write(response);
            }

            _responseMessage.StatusCode = _configuration.DefaultStatusCode;
            return _responseMessage;
        }
    }
}