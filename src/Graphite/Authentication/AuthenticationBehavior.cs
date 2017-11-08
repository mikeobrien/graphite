using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Authentication
{
    public class AuthenticationBehavior : BehaviorBase
    {
        private readonly HttpRequestMessage _requestMessage;
        private readonly HttpResponseMessage _responseMessage;
        private readonly IEnumerable<IAuthenticator> _authenticators;
        private readonly Configuration _configuration;
        private readonly ActionDescriptor _actionDescriptor;

        public AuthenticationBehavior(IBehaviorChain behaviorChain,
            HttpRequestMessage requestMessage, 
            HttpResponseMessage responseMessage,
            List<IAuthenticator> authenticators,
            Configuration configuration,
            ActionDescriptor actionDescriptor) : 
            base(behaviorChain)
        {
            _requestMessage = requestMessage;
            _responseMessage = responseMessage;
            _authenticators = authenticators;
            _configuration = configuration;
            _actionDescriptor = actionDescriptor;
        }

        public override bool ShouldRun()
        {
            return !_actionDescriptor.Action.IsGraphiteAction() ||
                !_configuration.ExcludeDiagnosticsFromAuthentication;
        }

        public override async Task<HttpResponseMessage> Invoke()
        {
            var authenticators = _actionDescriptor.Authenticators.ThatApply(_authenticators).ToList();
            if (!authenticators.Any())
            {
                if (_configuration.FailIfNoAuthenticatorsApplyToAction)
                    throw new GraphiteException("No authenticators registered.");
                return await BehaviorChain.InvokeNext();
            }

            var authorization = _requestMessage.Headers.Authorization;
            if (authorization == null) return GetUnauthorizedResponse(authenticators);

            var authenticator = authenticators.FirstOrDefault(x => 
                x.Scheme.EqualsUncase(authorization.Scheme));
            if (authenticator == null) return GetUnauthorizedResponse(authenticators);

            return authenticator.Authenticate(authorization.Parameter)
                ? await BehaviorChain.InvokeNext() 
                : GetUnauthorizedResponse(authenticators, authenticator);
        }

        protected virtual HttpResponseMessage GetUnauthorizedResponse(
            List<IAuthenticator> authenticators, IAuthenticator authenticator = null)
        {
            _responseMessage.StatusCode = HttpStatusCode.Unauthorized;
            _responseMessage.SafeSetReasonPhrase(authenticator?.UnauthorizedReasonPhrase ??
                _configuration.DefaultUnauthorizedStatusMessage);

            authenticators.ForEach(x => _responseMessage.AddAuthenticateHeader(
                x.Scheme, x.Realm ?? _configuration.DefaultAuthenticationRealm));

            return _responseMessage;
        }
    }
}
