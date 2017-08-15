using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Writers;

namespace Graphite.Actions
{
    public class ActionInvoker : IActionInvoker
    {
        private readonly IEnumerable<IRequestBinder> _requestBinders;
        private readonly IEnumerable<IResponseWriter> _writers;
        private readonly IEnumerable<IResponseStatus> _responseStatus;
        private readonly HttpResponseMessage _responseMessage;
        private readonly ActionDescriptor _actionDescriptor;

        public ActionInvoker(
            ActionDescriptor actionDescriptor,
            IEnumerable<IRequestBinder> requestBinders, 
            IEnumerable<IResponseWriter> writers,
            IEnumerable<IResponseStatus> responseStatus,
            HttpResponseMessage responseMessage)
        {
            _actionDescriptor = actionDescriptor;
            _requestBinders = requestBinders;
            _writers = writers;
            _responseStatus = responseStatus;
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
                if (writer != null)
                {
                    _responseMessage.SetStatus(_responseStatus, _actionDescriptor, ResponseState.Response);
                    return await writer.Write(response);
                }
                _responseMessage.SetStatus(_responseStatus, _actionDescriptor, ResponseState.NoWriter);
            }
            else _responseMessage.SetStatus(_responseStatus, _actionDescriptor, ResponseState.NoResponse);

            return _responseMessage;
        }
    }
}