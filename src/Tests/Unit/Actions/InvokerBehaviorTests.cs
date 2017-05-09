using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Actions
{
    [TestFixture]
    public class InvokerBehaviorTests
    {
        public interface IHandler
        {
            void NoParamsOrResponse();
            Task NoParamsOrResponseAsync();
            void Params(string param);
            Task ParamsAsync(string param);
            void Params(string param1, string param2);
            Task ParamsAsync(string param1, string param2);
            string Response();
            Task<string> ResponseAsync();
            string ParamsAndResponse(string param);
            Task<string> ParamsAndResponseAsync(string param);
        }

        [Test]
        public async Task Should_invoke_with_handler()
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.Response());
            var actionInvoker = Substitute.For<IActionInvoker>();
            var invokerBehavior = new InvokerBehavior(requestGraph.Container,
                requestGraph.ActionMethod, actionInvoker);
            var handler = Substitute.For<IHandler>();
            var responseMessage = new HttpResponseMessage();

            actionInvoker.Invoke(handler).Returns(responseMessage);
            requestGraph.UnderlyingContainer.Configure(x => x.For<IHandler>().Use(handler));

            var response = await invokerBehavior.Invoke();

            response.ShouldEqual(response);
        }
    }
}