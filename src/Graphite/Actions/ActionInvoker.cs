using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Writers;

namespace Graphite.Actions
{
    public class ActionInvoker : IActionInvoker
    {
        private readonly RequestContext _requestContext;
        private readonly IEnumerable<IRequestBinder> _requestBinders;
        private readonly IEnumerable<IResponseWriter> _writers;
        private readonly Configuration _configuration;

        public ActionInvoker(RequestContext requestContext,
            IEnumerable<IRequestBinder> requestBinders, 
            IEnumerable<IResponseWriter> writers,
            Configuration configuration)
        {
            _requestContext = requestContext;
            _requestBinders = requestBinders;
            _writers = writers;
            _configuration = configuration;
        }

        public virtual async Task<HttpResponseMessage> Invoke(object handler)
        {
            var actionArguments = new object[_requestContext.Action.Method.Parameters.Length];

            if (actionArguments.Any())
            {
                foreach (var binder in _requestBinders.ThatApplyTo(
                    actionArguments, _requestContext, _configuration))
                {
                    await binder.Bind(_configuration, _requestContext, actionArguments);
                }
            }

            var response = await _requestContext.Action.Invoke(handler, actionArguments);

            if (_requestContext.Route.HasResponse)
            {
                var writer = _writers.ThatApplyTo(response, _requestContext, _configuration);
                if (writer != null) return await writer.Write(
                    _configuration, _requestContext, response);
            }

            return new HttpResponseMessage(_configuration.DefaultStatusCode);
        }
    }
}