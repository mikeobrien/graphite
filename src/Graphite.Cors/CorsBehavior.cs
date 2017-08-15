using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Cors
{
    public class CorsBehavior : BehaviorBase
    {
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;
        private readonly ICorsEngine _corsEngine;
        private readonly CorsConfiguration _corsConfiguration;
        private readonly ActionDescriptor _actionDescriptor;
        private readonly HttpRequestMessage _requestMessage;
        private readonly IEnumerable<ICorsPolicySource> _policySources;

        public CorsBehavior(IBehaviorChain behaviorChain, 
            ICorsEngine corsEngine, CorsConfiguration corsConfiguration, 
            ActionDescriptor actionDescriptor, HttpRequestMessage requestMessage, 
            IEnumerable<ICorsPolicySource> policySources, Configuration configuration,
            HttpConfiguration httpConfiguration) : base(behaviorChain)
        {
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
            _corsEngine = corsEngine;
            _corsConfiguration = corsConfiguration;
            _actionDescriptor = actionDescriptor;
            _requestMessage = requestMessage;
            _policySources = policySources;
        }

        public override async Task<HttpResponseMessage> Invoke()
        {
            var corsRequestContext = _requestMessage.GetCorsRequestContext();
            var corsPolicy = _policySources.ThatApplies(_corsConfiguration,
                _actionDescriptor, _configuration, _httpConfiguration)?.CreatePolicy();

            if (corsPolicy == null) return await BehaviorChain.InvokeNext();

            var preflight = corsRequestContext.IsPreflight;

            if (!preflight && corsRequestContext.Origin.IsNullOrEmpty())
            {
                return corsPolicy.AllowRequestsWithoutOriginHeader
                    ? await BehaviorChain.InvokeNext()
                    : _requestMessage.CreateResponse(HttpStatusCode.BadRequest);
            }

            return await HandleRequest(preflight, corsRequestContext, corsPolicy);
        }

        private async Task<HttpResponseMessage> HandleRequest(bool preflight, 
            CorsRequestContext requestContext, GraphiteCorsPolicy corsPolicy)
        {
            if (preflight && !_configuration.SupportedHttpMethods.Contains(
                    requestContext.AccessControlRequestMethod))
                return await FailRequest(corsPolicy, true);

            var result = _corsEngine.EvaluatePolicy(requestContext, corsPolicy);
            
            if (result == null || !result.IsValid)
                return await FailRequest(corsPolicy, preflight, 
                    result?.ErrorMessages.Join(" "));

            var response = !preflight || corsPolicy.AllowOptionRequestsToPassThrough
                ? await BehaviorChain.InvokeNext()
                : _requestMessage.CreateResponse();

            response.WriteCorsHeaders(result);
            return response;
        }

        private async Task<HttpResponseMessage> FailRequest(GraphiteCorsPolicy corsPolicy, 
            bool preflight, string message = null)
        {
            if (!preflight && corsPolicy.AllowRequestsThatFailCors)
                return await BehaviorChain.InvokeNext();
            if (preflight && corsPolicy.AllowOptionRequestsToPassThrough)
                return BadRequest(await BehaviorChain.InvokeNext(), message);
            return BadRequest(message: message);
        }

        private HttpResponseMessage BadRequest(HttpResponseMessage response = null, string message = null)
        {
            response = response ?? _requestMessage.CreateResponse();
            response.StatusCode = HttpStatusCode.BadRequest;
            if (message.IsNotNullOrEmpty() && response.Content == null)
                response.Content = new StringContent(message, 
                    _configuration.DefaultEncoding, MimeTypes.TextPlain);
            return response;
        }
    }
}
