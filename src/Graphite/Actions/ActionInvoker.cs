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
        private readonly IEnumerable<IResponseHeaders> _responseHeaders;
        private readonly HttpResponseMessage _responseMessage;
        private readonly ActionDescriptor _actionDescriptor;

        public ActionInvoker(
            ActionDescriptor actionDescriptor,
            IEnumerable<IRequestBinder> requestBinders, 
            IEnumerable<IResponseWriter> writers,
            IEnumerable<IResponseStatus> responseStatus,
            IEnumerable<IResponseHeaders> responseHeaders, 
            HttpResponseMessage responseMessage)
        {
            _actionDescriptor = actionDescriptor;
            _requestBinders = requestBinders;
            _writers = writers;
            _responseStatus = responseStatus;
            _responseHeaders = responseHeaders;
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
                    var result = await binder.Bind(requestBinderContext);
                    switch (result.Status)
                    {
                        case BindingStatus.Failure:
                            return SetStatusAndHeaders(ResponseState.BindingFailure, result.ErrorMessage);
                        case BindingStatus.NoReader:
                            return SetStatusAndHeaders(ResponseState.NoReader);
                    }
                }
            }

            var response = await _actionDescriptor.Action.Invoke(handler, actionArguments);

            if (response is HttpResponseMessage)
                return SetHeaders(response.As<HttpResponseMessage>());

            if (!_actionDescriptor.Route.HasResponse)
                return SetStatusAndHeaders(ResponseState.NoResponse);

            var writer = _writers.ThatApply(response, _actionDescriptor).FirstOrDefault();
            if (writer == null) return SetStatusAndHeaders(ResponseState.NoWriter);
            SetStatusAndHeaders(ResponseState.HasResponse);
            return await writer.Write(response);
        }

        private HttpResponseMessage SetStatusAndHeaders(
            ResponseState responseState, string errorMessage = null)
        {
            _responseMessage.SetStatus(_responseStatus, 
                _actionDescriptor, responseState, errorMessage);
            return SetHeaders(_responseMessage);
        }

        private HttpResponseMessage SetHeaders(HttpResponseMessage message)
        {
            message.ApplyHeaders(_responseHeaders, _actionDescriptor);
            return message;
        }
    }
}