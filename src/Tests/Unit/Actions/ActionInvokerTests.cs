using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Reflection;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Common.Fakes;

namespace Tests.Unit.Actions
{
    [TestFixture]
    public class ActionInvokerTests
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
            HttpResponseMessage HttpResponseMessageResponse();
            Task<string> ResponseAsync();
            string ParamsAndResponse(string param);
            Task<string> ParamsAndResponseAsync(string param);
        }

        public static object[][] ActionTestCases = TestCaseSource
            .Create<ActionMethod>(x => x
                .Add(ActionMethod.From<IHandler>(h => h.NoParamsOrResponse()))
                .Add(ActionMethod.From<IHandler>(h => h.NoParamsOrResponseAsync()))
                .Add(ActionMethod.From<IHandler>(h => h.Params(null)))
                .Add(ActionMethod.From<IHandler>(h => h.Params(null, null)))
                .Add(ActionMethod.From<IHandler>(h => h.ParamsAsync(null)))
                .Add(ActionMethod.From<IHandler>(h => h.ParamsAsync(null, null)))
                .Add(ActionMethod.From<IHandler>(h => h.Response()))
                .Add(ActionMethod.From<IHandler>(h => h.ResponseAsync()))
                .Add(ActionMethod.From<IHandler>(h => h.ParamsAndResponse(null)))
                .Add(ActionMethod.From<IHandler>(h => h.ParamsAndResponseAsync(null))));

        [TestCaseSource(nameof(ActionTestCases))]
        public async Task Should_call_action(ActionMethod actionMethod)
        {
            var requestGraph = RequestGraph.CreateFor(actionMethod);
            var routeDescriptor = requestGraph.GetRouteDescriptor();

            requestGraph.AddResponseWriter1(c => c.Response.ToString().CreateTextResponse().ToTaskResult());
            requestGraph.AddRequestBinder1(c => SetArguments(requestGraph.ActionMethod, 
                c, (a, p) => a[p.Position] = $"value{p.Position}"));
            requestGraph.AddRequestBinder2(c => SetArguments(requestGraph.ActionMethod, 
                c, (a, p) => a[p.Position] += $"-{p.Position}"));

            var invoker = CreateInvoker(requestGraph);
            var handler = Substitute.For<IHandler>();

            handler.Response().ReturnsForAnyArgs(x => "response");
            handler.ResponseAsync().ReturnsForAnyArgs(x => "response".ToTaskResult());
            handler.ParamsAndResponse(null).ReturnsForAnyArgs(x => "response");
            handler.ParamsAndResponseAsync(null).ReturnsForAnyArgs(x => "response".ToTaskResult());

            var response = await invoker.Invoke(handler);

            var expectedArguments = actionMethod.MethodDescriptor.Parameters.Select(p =>
                $"value{p.Position}-{p.Position}").Cast<object>().ToArray();

            await actionMethod.Invoke(handler.Received(1), expectedArguments);

            if (routeDescriptor.HasResponse)
            {
                response.StatusCode.ShouldEqual(HttpStatusCode.OK);
                var responseText = await response.Content.ReadAsStringAsync();
                responseText.ShouldEqual("response");
            }
            else response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
        }

        private static Task SetArguments(ActionMethod actionMethod, 
            RequestBinderContext context, Action<object[], ParameterDescriptor> set)
        {
            actionMethod.MethodDescriptor.Parameters.ForEach(pi => set(context.ActionArguments, pi));
            return Task.CompletedTask;
        }

        [Test]
        public async Task Should_not_run_binders_where_instance_does_not_apply()
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.Params(null, null));

            requestGraph.AddRequestBinder1(c => SetArguments(requestGraph.ActionMethod,
                c, (a, p) => a[0] = "binder1"));
            requestGraph.AddRequestBinder2(c => SetArguments(requestGraph.ActionMethod,
                c, (a, p) => a[1] += "binder2"), instanceAppliesTo: x => false);

            var invoker = CreateInvoker(requestGraph);
            var handler = Substitute.For<IHandler>();

            await invoker.Invoke(handler);

            var expectedArguments = new object[] { "binder1", null };

            await requestGraph.ActionMethod.Invoke(handler.Received(1), expectedArguments);

            requestGraph.RequestBinder1.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestBinder1.AppliesToContext.ActionArguments.ShouldEqual(expectedArguments);
            requestGraph.RequestBinder1.BindCalled.ShouldBeTrue();
            requestGraph.RequestBinder1.BindContext.ActionArguments.ShouldEqual(expectedArguments);

            requestGraph.RequestBinder2.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestBinder2.AppliesToContext.ActionArguments.ShouldEqual(expectedArguments);
            requestGraph.RequestBinder2.BindCalled.ShouldBeFalse();
        }

        [Test]
        public async Task Should_not_run_binders_that_do_not_apply_in_configuration()
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.Params(null, null));

            requestGraph.AddRequestBinder1(c => SetArguments(requestGraph.ActionMethod,
                c, (a, p) => a[0] = "binder1"));
            requestGraph.AddRequestBinder2(c => SetArguments(requestGraph.ActionMethod,
                c, (a, p) => a[1] += "binder2"), configAppliesTo: x => false);

            var invoker = CreateInvoker(requestGraph);
            var handler = Substitute.For<IHandler>();

            await invoker.Invoke(handler);

            var expectedArguments = new object[] { "binder1", null };

            await requestGraph.ActionMethod.Invoke(handler.Received(1), expectedArguments);

            requestGraph.RequestBinder1.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestBinder1.AppliesToContext.ActionArguments.ShouldEqual(expectedArguments);
            requestGraph.RequestBinder1.BindCalled.ShouldBeTrue();
            requestGraph.RequestBinder1.BindContext.ActionArguments.ShouldEqual(expectedArguments);

            requestGraph.RequestBinder2.AppliesToCalled.ShouldBeFalse();
            requestGraph.RequestBinder2.BindCalled.ShouldBeFalse();
        }

        [Test]
        public async Task Should_use_first_writer_that_applies()
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.Response());

            requestGraph.AddResponseWriter1(x => $"{x.Response}1".CreateTextResponse()
                .ToTaskResult(), instanceAppliesTo: x => false);
            requestGraph.AddResponseWriter2(x => $"{x.Response}2".CreateTextResponse().ToTaskResult());

            var invoker = CreateInvoker(requestGraph);
            var handler = Substitute.For<IHandler>();

            handler.Response().ReturnsForAnyArgs(x => "response");

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            var responseText = await response.Content.ReadAsStringAsync();
            responseText.ShouldEqual("response2");
        }

        [Test]
        public async Task Should_use_default_writer_if_set_and_no_writers_apply()
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.Response());

            requestGraph.Configuration.ResponseWriters.DefaultIs<TestResponseWriter2>();

            requestGraph.AddResponseWriter1(x => $"{x.Response}1".CreateTextResponse()
                .ToTaskResult(), instanceAppliesTo: x => false);
            requestGraph.AddResponseWriter2(x => $"{x.Response}2".CreateTextResponse()
                .ToTaskResult(), instanceAppliesTo: x => false);

            var invoker = CreateInvoker(requestGraph);
            var handler = Substitute.For<IHandler>();

            handler.Response().ReturnsForAnyArgs(x => "response");

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            var responseText = await response.Content.ReadAsStringAsync();
            responseText.ShouldEqual("response2");
        }

        [Test]
        public async Task Should_directly_return_http_response_message()
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.HttpResponseMessageResponse());

            var invoker = CreateInvoker(requestGraph);
            var handler = Substitute.For<IHandler>();
            var responseMessage = new HttpResponseMessage();

            handler.HttpResponseMessageResponse().ReturnsForAnyArgs(x => responseMessage);

            var response = await invoker.Invoke(handler);

            response.ShouldEqual(responseMessage);
        }

        [Test]
        public async Task Should_return_empty_response_if_no_writers_apply(
            [Values(null, HttpStatusCode.OK)] HttpStatusCode? statusCode)
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.Response());

            if (statusCode.HasValue) requestGraph.Configuration.DefaultStatusCode = statusCode.Value;
            var invoker = CreateInvoker(requestGraph);

            var handler = Substitute.For<IHandler>();
            handler.Response().ReturnsForAnyArgs(x => "response");

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(requestGraph.Configuration.DefaultStatusCode);
        }

        private ActionInvoker CreateInvoker(RequestGraph requestGraph)
        {
            return new ActionInvoker(requestGraph.GetActionConfigurationContext(),
                requestGraph.ActionMethod, requestGraph.GetRouteDescriptor(),
                requestGraph.RequestBinders, requestGraph.ResponseWriters,
                requestGraph.GetHttpResponseMessage());
        }
    }
}