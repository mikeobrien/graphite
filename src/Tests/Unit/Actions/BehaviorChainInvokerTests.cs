using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;
using NUnit.Framework;
using Should;
using StructureMap.Building;
using Tests.Common;
using Tests.Common.Fakes;

namespace Tests.Unit.Actions
{
    [TestFixture]
    public class BehaviorChainInvokerTests
    {
        public class Handler
        {
            public void Get(string urlParam, string queryParam) { }
        }

        private RequestGraph _requestGraph;
        private TestInvokerBehavior _invokerBehavior;
        private HttpResponseMessage _responseMessage;
        private BehaviorChainInvoker _invoker;
        private Configuration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _requestGraph = RequestGraph.CreateFor<Handler>(x => x.Get(null, null));
            _responseMessage = new HttpResponseMessage();
            _invokerBehavior = new TestInvokerBehavior(_responseMessage);
            _configuration.DefaultBehavior.Set(_invokerBehavior);
            _invoker = new BehaviorChainInvoker(_requestGraph.Container, _configuration);
        }

        public class RegistrationLoggingBehavior : TestBehavior
        {
            public RegistrationLoggingBehavior(
                IBehavior innerBehavior, Logger logger,
                HttpRequestMessage request,
                RequestContext requestContext,
                ActionMethod actionMethod,
                RouteDescriptor routeDescriptor,
                HttpRequestContext httpRequestContext,
                RequestCancellation requestCancellation,
                UrlParameters urlParameters,
                QuerystringParameters querystringParameters,
                SomeType someInstance)
            {
                InnerBehavior = innerBehavior;
                logger.Write(request);
                logger.Write(requestContext);
                logger.Write(actionMethod);
                logger.Write(routeDescriptor);
                logger.Write(httpRequestContext);
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
            _requestGraph.Configuration.Behaviors.Append<RegistrationLoggingBehavior>();
            _requestGraph.Url = "http://fark.com/urlparamvalue?queryparam=queryvalue";
            _requestGraph.UrlTemplate = "{urlParam}";
            _requestGraph.AddParameter("queryParam");

            var log = new Logger();
            _requestGraph.UnderlyingContainer.Configure(x =>
            {
                x.For<Logger>().Use(log);
            });

            var requestMessage = _requestGraph.GetHttpRequestMessage();
            var actionDescriptor = _requestGraph.GetActionDescriptor();

            actionDescriptor.Registry.Register(someInstance);

            await _invoker.Invoke(actionDescriptor,
                requestMessage, _requestGraph.CancellationToken);

            log.ShouldContain(requestMessage);
            log.ShouldContain(_requestGraph.ActionMethod);
            log.ShouldContain(actionDescriptor.Route);
            log.ShouldContain(requestMessage.GetRequestContext());
            log.ShouldContain(someInstance);

            var urlParameters = log.OfType<UrlParameters>().FirstOrDefault();
            urlParameters.ShouldNotBeNull();

            urlParameters["urlParam"].ShouldEqual("urlparamvalue");

            var querystringParameters = log.OfType<QuerystringParameters>().FirstOrDefault();
            querystringParameters.ShouldNotBeNull();

            querystringParameters["queryparam"].ShouldOnlyContain("queryvalue");

            var requestCancellation = log.OfType<RequestCancellation>().FirstOrDefault();
            requestCancellation.ShouldNotBeNull();

            requestCancellation.Token.ShouldEqual(_requestGraph.CancellationToken);

            var requestContext = log.OfType<RequestContext>().FirstOrDefault();
            requestContext.ShouldNotBeNull();

            requestContext.Action.ShouldEqual(_requestGraph.ActionMethod);
            requestContext.Route.ShouldEqual(actionDescriptor.Route);
            requestContext.UrlParameters.ShouldEqual(urlParameters);
            requestContext.QuerystringParameters.ShouldEqual(querystringParameters);
            requestContext.Behaviors.ShouldEqual(actionDescriptor.Behaviors);
            requestContext.RequestMessage.ShouldEqual(requestMessage);
            requestContext.HttpConfiguration.ShouldEqual(requestMessage.GetConfiguration());
            requestContext.CancellationToken.ShouldEqual(_requestGraph.CancellationToken);
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
            public DisposableBehavior(IBehavior innerBehavior, Disposable disposable)
            {
                InnerBehavior = innerBehavior;
            }
        }

        [Test]
        public async Task Should_dispose_scoped_container()
        {
            _requestGraph.Configuration.Behaviors.Append<DisposableBehavior>();

            var log = new Logger();
            _requestGraph.UnderlyingContainer.Configure(x =>
            {
                x.For<Disposable>().Use<Disposable>();
                x.For<Logger>().Use(log);
            });

            await _invoker.Invoke(_requestGraph.GetActionDescriptor(), 
                _requestGraph.GetHttpRequestMessage(),
                _requestGraph.CancellationToken);

            log.ShouldContain(nameof(Disposable));
        }

        [Test]
        public async Task Should_invoke_with_empty_behavior_chain()
        {
            _requestGraph.UnderlyingContainer.Configure(x => x.For<IBehavior>().Use(_invokerBehavior));

            var response = await _invoker.Invoke(_requestGraph.GetActionDescriptor(),
                _requestGraph.GetHttpRequestMessage(),
                _requestGraph.CancellationToken);

            response.ShouldEqual(_responseMessage);
        }

        public class Behavior1 : TestLoggingBehavior
        {
            public Behavior1(IBehavior innerBehavior, Logger logger)
            {
                Logger = logger;
                InnerBehavior = innerBehavior;
            }
        }

        public class Behavior2 : TestLoggingBehavior
        {
            public Behavior2(IBehavior innerBehavior, Logger logger)
            {
                Logger = logger;
                InnerBehavior = innerBehavior;
            }
        }

        public class Behavior3 : TestLoggingBehavior
        {
            public Behavior3(IBehavior innerBehavior, Logger logger)
            {
                Logger = logger;
                InnerBehavior = innerBehavior;
            }
        }

        [Test]
        public async Task Should_invoke_behaviors_in_correct_order()
        {
            _requestGraph.Configuration.Behaviors.Append<Behavior1>();
            _requestGraph.Configuration.Behaviors.Append<Behavior2>();
            _requestGraph.Configuration.Behaviors.Append<Behavior3>();

            var log = new Logger();
            _requestGraph.UnderlyingContainer.Configure(x =>
            {
                x.For<Logger>().Use(log);
            });

            var response = await _invoker.Invoke(_requestGraph.GetActionDescriptor(),
                _requestGraph.GetHttpRequestMessage(),
                _requestGraph.CancellationToken);

            response.ShouldEqual(_responseMessage);

            log.ShouldOnlyContain(typeof(Behavior1), typeof(Behavior2), typeof(Behavior3));
        }

        public class ShouldNotRunBehavior : TestLoggingBehavior
        {
            public ShouldNotRunBehavior(IBehavior innerBehavior, Logger logger)
            {
                Logger = logger;
                InnerBehavior = innerBehavior;
                ShouldRunFlag = false;
            }
        }

        [TestCase(typeof(ShouldNotRunBehavior), typeof(Behavior1), typeof(Behavior2))]
        [TestCase(typeof(Behavior1), typeof(ShouldNotRunBehavior), typeof(Behavior2))]
        [TestCase(typeof(Behavior1), typeof(Behavior2), typeof(ShouldNotRunBehavior))]
        public async Task Should_not_invoke_behaviors_that_should_not_run(Type behaviorA, Type behaviorB, Type behaviorC)
        {
            _requestGraph.Configuration.Behaviors.Append<IBehavior, BehaviorContext>(behaviorA);
            _requestGraph.Configuration.Behaviors.Append<IBehavior, BehaviorContext>(behaviorB);
            _requestGraph.Configuration.Behaviors.Append<IBehavior, BehaviorContext>(behaviorC);

            var log = new Logger();
            _requestGraph.UnderlyingContainer.Configure(x =>
            {
                x.For<Logger>().Use(log);
            });

            var response = await _invoker.Invoke(_requestGraph.GetActionDescriptor(),
                _requestGraph.GetHttpRequestMessage(),
                _requestGraph.CancellationToken);

            response.ShouldEqual(_responseMessage);

            log.ShouldOnlyContain(typeof(Behavior1), typeof(Behavior2));
        }

        public class BehaviorException : Exception { }

        public class InitFailureBehavior : BehaviorBase
        {
            public InitFailureBehavior()
            {
                throw new BehaviorException();
            }

            public override async Task<HttpResponseMessage> Invoke()
            {
                return new HttpResponseMessage();
            }
        }

        [Test]
        public async Task Should_wrap_invoker_behavior_with_runtime_init_exception_during_behavior_chain_construction()
        {
            _configuration.DefaultBehavior.Set<InitFailureBehavior>();

            var exception = await _invoker.Should()
                .Throw<GraphiteRuntimeInitializationException>(x =>
                    x.Invoke(_requestGraph.GetActionDescriptor(),
                        _requestGraph.GetHttpRequestMessage(),
                        _requestGraph.CancellationToken));

            exception.GetChain().ShouldOnlyContainTypes<
                GraphiteRuntimeInitializationException,
                StructureMapBuildException,
                BehaviorException>();
        }

        public class FailureBehavior : BehaviorBase
        {
            public override async Task<HttpResponseMessage> Invoke()
            {
                throw new BehaviorException();
            }
        }

        [Test]
        public async Task Should_not_wrap_invoker_behavior_with_runtime_init_exception_on_invoke()
        {
            _configuration.DefaultBehavior.Set<FailureBehavior>();
            _requestGraph.Configuration.DefaultErrorHandlerEnabled = false;

            await _invoker.Should().Throw<BehaviorException>(x =>
                x.Invoke(_requestGraph.GetActionDescriptor(),
                        _requestGraph.GetHttpRequestMessage(),
                        _requestGraph.CancellationToken));
        }

        [Test]
        public async Task Should_wrap_behavior_with_runtime_init_exception_during_behavior_chain_construction()
        {
            _requestGraph.Configuration.Behaviors.Append<InitFailureBehavior>();

            var exception = await _invoker.Should().Throw<GraphiteRuntimeInitializationException>(x =>
                x.Invoke(_requestGraph.GetActionDescriptor(),
                        _requestGraph.GetHttpRequestMessage(),
                        _requestGraph.CancellationToken));

            exception.GetChain().ShouldOnlyContainTypes<
                GraphiteRuntimeInitializationException,
                StructureMapBuildException,
                BehaviorException>();
        }

        [Test]
        public async Task Should_not_wrap_behavior_with_runtime_init_exception_on_invoke()
        {
            _requestGraph.UnderlyingContainer.Configure(x => x.For<IBehavior>().Use<TestInvokerBehavior>());
            _requestGraph.Configuration.DefaultErrorHandlerEnabled = false;
            _requestGraph.Configuration.Behaviors.Append<FailureBehavior>();

            await _invoker.Should().Throw<BehaviorException>(x =>
                x.Invoke(_requestGraph.GetActionDescriptor(),
                        _requestGraph.GetHttpRequestMessage(),
                        _requestGraph.CancellationToken));
        }
    }
}
