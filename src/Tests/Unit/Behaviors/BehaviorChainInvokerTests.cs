using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.DependencyInjection;
using Graphite.Http;
using Graphite.Routing;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Common.Fakes;

namespace Tests.Unit.Behaviors
{
    [TestFixture]
    public class BehaviorChainInvokerTests
    {
        public class Handler
        {
            public void Get(string urlParam, string queryParam) { }
        }

        private RequestGraph _requestGraph;
        private BehaviorChainInvoker _invoker;

        [SetUp]
        public void Setup()
        {
            _requestGraph = RequestGraph.CreateFor<Handler>(x => x.Get(null, null));
            _requestGraph.Configure(x => x
                .ReturnErrorMessages()
                .WithDefaultBehavior<TestInvokerBehavior>());
            _invoker = new BehaviorChainInvoker(_requestGraph.Configuration, _requestGraph.Container);
        }

        public class RegistrationLoggingBehavior : TestBehavior
        {
            public RegistrationLoggingBehavior(
                IContainer container,
                IBehaviorChain behaviorChainChain, Logger logger,
                HttpRequestMessage requestMessage,
                HttpResponseMessage responseMessage,
                HttpResponseHeaders responseHeaders,
                HttpConfiguration httpConfiguration,
                HttpRequestContext httpRequestContext,
                ActionDescriptor actionDescriptor,
                ActionMethod actionMethod,
                RouteDescriptor routeDescriptor,
                RequestCancellation requestCancellation,
                UrlParameters urlParameters,
                QuerystringParameters querystringParameters,
                SomeType someInstance)
            {
                BehaviorChain = behaviorChainChain;
                logger.Write(container);
                logger.Write(requestMessage);
                logger.Write(responseMessage);
                logger.Write(responseHeaders);
                logger.Write(actionMethod);
                logger.Write(actionDescriptor);
                logger.Write(routeDescriptor);
                logger.Write(httpRequestContext);
                logger.Write(httpConfiguration);
                logger.Write(requestCancellation);
                logger.Write(urlParameters);
                logger.Write(querystringParameters);
                logger.Write(someInstance);
            }
        }

        public class SomeType { }

        [Test]
        public async Task Should_register_request_objects_in_scoped_container()
        {
            var someInstance = new SomeType();
            _requestGraph.Configuration.Behaviors.Configure(c => c.Append<RegistrationLoggingBehavior>());
            _requestGraph.Url = "http://fark.com/urlparamvalue?queryparam=queryvalue";
            _requestGraph.UrlTemplate = "{urlParam}";
            _requestGraph.AddParameters("queryParam");

            var log = new Logger();
            _requestGraph.UnderlyingContainer.Configure(x =>
            {
                x.For<Logger>().Use(log);
            });

            var requestMessage = _requestGraph.GetHttpRequestMessage();
            var actionDescriptor = _requestGraph.GetActionDescriptor();

            actionDescriptor.Registry.Register(someInstance);

            var response = await _invoker.Invoke(actionDescriptor,
                requestMessage, _requestGraph.CancellationToken);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK, 
                response.Content?.ReadAsStringAsync().Result);

            log.ShouldContain(requestMessage);
            log.ShouldContain(_requestGraph.ActionMethod);
            log.ShouldContain(actionDescriptor);
            log.ShouldContain(actionDescriptor.Route);
            log.ShouldContain(requestMessage.GetRequestContext());
            log.ShouldContain(someInstance);

            log.ShouldContain(x => x is HttpResponseMessage);
            log.ShouldContain(x => x is HttpResponseHeaders);
            log.ShouldContain(x => x is HttpConfiguration);
            log.ShouldContain(x => x is HttpRequestContext);

            var container = log.OfType<IContainer>().FirstOrDefault();
            container.ShouldNotBeNull();
            container.ShouldNotEqual(_requestGraph.Container);

            var urlParameters = log.OfType<UrlParameters>().FirstOrDefault();
            urlParameters.ShouldNotBeNull();

            urlParameters["urlParam"].ShouldEqual("urlparamvalue");

            var querystringParameters = log.OfType<QuerystringParameters>().FirstOrDefault();
            querystringParameters.ShouldNotBeNull();

            querystringParameters["queryparam"].ShouldOnlyContain("queryvalue");

            var requestCancellation = log.OfType<RequestCancellation>().FirstOrDefault();
            requestCancellation.ShouldNotBeNull();

            requestCancellation.Token.ShouldEqual(_requestGraph.CancellationToken);
        }

        public class Disposable : IDisposable
        {
            private readonly Logger _logger;

            public Disposable(Logger logger)
            {
                _logger = logger;
            }

            public void Dispose()
            {
                _logger.Write(GetType().Name);
            }
        }

        public class DisposableBehavior : TestBehavior
        {
            public DisposableBehavior(IBehaviorChain behaviorChain, Disposable disposable)
            {
                BehaviorChain = behaviorChain;
            }
        }

        [Test]
        public async Task Should_dispose_scoped_container()
        {
            _requestGraph.Configuration.Behaviors.Configure(c => c.Append<DisposableBehavior>());

            var log = new Logger();
            _requestGraph.UnderlyingContainer.Configure(x =>
            {
                x.For<Disposable>().Use<Disposable>();
                x.For<Logger>().Use(log);
            });
            var request = _requestGraph.GetHttpRequestMessage();

            var response = await _invoker.Invoke(_requestGraph.GetActionDescriptor(),
                request, _requestGraph.CancellationToken);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK,
                response.Content?.ReadAsStringAsync().Result);

            request.DisposeRequestResources();

            log.ShouldContain(nameof(Disposable));
        }

        [Test]
        public async Task Should_invoke_with_empty_behavior_chain()
        {
            var response = await _invoker.Invoke(_requestGraph.GetActionDescriptor(),
                _requestGraph.GetHttpRequestMessage(),
                _requestGraph.CancellationToken);

            response.ShouldNotBeNull();
            response.StatusCode.ShouldEqual(HttpStatusCode.OK,
                response.Content?.ReadAsStringAsync().Result);
        }

        public class Behavior1 : TestLoggingBehavior
        {
            public Behavior1(IBehaviorChain behaviorChain, Logger logger)
            {
                Logger = logger;
                BehaviorChain = behaviorChain;
            }
        }

        public class Behavior2 : TestLoggingBehavior
        {
            public Behavior2(IBehaviorChain behaviorChain, Logger logger)
            {
                Logger = logger;
                BehaviorChain = behaviorChain;
            }
        }

        public class Behavior3 : TestLoggingBehavior
        {
            public Behavior3(IBehaviorChain behaviorChain, Logger logger)
            {
                Logger = logger;
                BehaviorChain = behaviorChain;
            }
        }

        [Test]
        public async Task Should_invoke_behaviors_in_correct_order()
        {
            _requestGraph.Configuration.Behaviors.Configure(c => c
                .Append<Behavior1>()
                .Append<Behavior2>()
                .Append<Behavior3>());

            var log = new Logger();
            _requestGraph.UnderlyingContainer.Configure(x =>
            {
                x.For<Logger>().Use(log);
            });

            var response = await _invoker.Invoke(_requestGraph.GetActionDescriptor(),
                _requestGraph.GetHttpRequestMessage(),
                _requestGraph.CancellationToken);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK,
                response.Content?.ReadAsStringAsync().Result);

            log.ShouldOnlyContain(typeof(Behavior1), typeof(Behavior2), typeof(Behavior3));
        }

        public class ShouldNotRunBehavior : TestLoggingBehavior
        {
            public ShouldNotRunBehavior(IBehaviorChain behaviorChain, Logger logger)
            {
                Logger = logger;
                BehaviorChain = behaviorChain;
                ShouldRunFlag = false;
            }
        }

        [TestCase(typeof(ShouldNotRunBehavior), typeof(Behavior1), typeof(Behavior2))]
        [TestCase(typeof(Behavior1), typeof(ShouldNotRunBehavior), typeof(Behavior2))]
        [TestCase(typeof(Behavior1), typeof(Behavior2), typeof(ShouldNotRunBehavior))]
        public async Task Should_not_invoke_behaviors_that_should_not_run(Type behaviorA, Type behaviorB, Type behaviorC)
        {
            _requestGraph.Configuration.Behaviors.Configure(x => x
                .Append<IBehavior, ActionConfigurationContext>(behaviorA)
                .Append<IBehavior, ActionConfigurationContext>(behaviorB)
                .Append<IBehavior, ActionConfigurationContext>(behaviorC));

            var log = new Logger();
            _requestGraph.UnderlyingContainer.Configure(x =>
            {
                x.For<Logger>().Use(log);
            });

            var response = await _invoker.Invoke(_requestGraph.GetActionDescriptor(),
                _requestGraph.GetHttpRequestMessage(),
                _requestGraph.CancellationToken);

            response.ShouldNotBeNull();
            response.StatusCode.ShouldEqual(HttpStatusCode.OK,
                response.Content?.ReadAsStringAsync().Result);

            log.ShouldOnlyContain(typeof(Behavior1), typeof(Behavior2));
        }

        public class BehaviorException : Exception { }

        public class InitFailureBehavior : IBehavior
        {
            public InitFailureBehavior()
            {
                throw new BehaviorException();
            }

            public bool ShouldRun()
            {
                throw new NotImplementedException();
            }

            public async Task<HttpResponseMessage> Invoke()
            {
                return new HttpResponseMessage();
            }
        }
    }
}
