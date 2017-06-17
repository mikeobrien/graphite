using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Extensions;

namespace Graphite.Authentication
{
    public class AuthenticationBehavior : BehaviorBase
    {
        private readonly HttpRequestMessage _requestMessage;
        private readonly HttpResponseMessage _responseMessage;
        private readonly IEnumerable<IAuthenticator> _authenticators;
        private readonly Configuration _configuration;
        private readonly ActionConfigurationContext _actionConfigurationContext;

        public AuthenticationBehavior(IBehavior innerBehavior,
            HttpRequestMessage requestMessage, 
            HttpResponseMessage responseMessage,
            List<IAuthenticator> authenticators,
            Configuration configuration,
            ActionConfigurationContext actionConfigurationContext) : 
            base(innerBehavior)
        {
            _requestMessage = requestMessage;
            _responseMessage = responseMessage;
            _authenticators = authenticators;
            _configuration = configuration;
            _actionConfigurationContext = actionConfigurationContext;
        }

        public override async Task<HttpResponseMessage> Invoke()
        {
            var authenticators = _authenticators.ThatApplyTo(_actionConfigurationContext).ToList();
            if (!authenticators.Any()) throw new GraphiteException("No authenticators registered.");

            var authorization = _requestMessage.Headers.Authorization;
            if (authorization == null) return GetUnauthorizedResponse(authenticators);

            var authenticator = authenticators.FirstOrDefault(x => 
                x.Scheme.EqualsIgnoreCase(authorization.Scheme));
            if (authenticator == null) return GetUnauthorizedResponse(authenticators);

            return authenticator.Authenticate(authorization.Parameter)
                ? await InnerBehavior.Invoke() 
                : GetUnauthorizedResponse(authenticators, authenticator);
        }

        protected virtual HttpResponseMessage GetUnauthorizedResponse(
            List<IAuthenticator> authenticators, IAuthenticator authenticator = null)
        {
            _responseMessage.StatusCode = HttpStatusCode.Unauthorized;
            _responseMessage.ReasonPhrase = authenticator?.UnauthorizedStatusMessage ??
                _configuration.DefaultUnauthorizedStatusMessage;

            authenticators.ForEach(x => _responseMessage.AddAuthenticateHeader(
                x.Scheme, x.Realm ?? _configuration.DefaultAuthenticationRealm));

            return _responseMessage;
        }
    }
}
