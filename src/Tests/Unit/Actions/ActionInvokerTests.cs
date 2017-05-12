using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Reflection;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;

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
            Task<string> ResponseAsync();
            string ParamsAndResponse(string param);
            Task<string> ParamsAndResponseAsync(string param);
        }

        public static object[][] ActionTestCases = TestCaseSource
            .Create<ActionMethod>(x => x
                .Add(Type<IHandler>.Expression(h => h.NoParamsOrResponse()).ToActionMethod())
                .Add(Type<IHandler>.Expression(h => h.NoParamsOrResponseAsync()).ToActionMethod())
                .Add(Type<IHandler>.Expression(h => h.Params(null)).ToActionMethod())
                .Add(Type<IHandler>.Expression(h => h.Params(null, null)).ToActionMethod())
                .Add(Type<IHandler>.Expression(h => h.ParamsAsync(null)).ToActionMethod())
                .Add(Type<IHandler>.Expression(h => h.ParamsAsync(null, null)).ToActionMethod())
                .Add(Type<IHandler>.Expression(h => h.Response()).ToActionMethod())
                .Add(Type<IHandler>.Expression(h => h.ResponseAsync()).ToActionMethod())
                .Add(Type<IHandler>.Expression(h => h.ParamsAndResponse(null)).ToActionMethod())
                .Add(Type<IHandler>.Expression(h => h.ParamsAndResponseAsync(null)).ToActionMethod()));

        [TestCaseSource(nameof(ActionTestCases))]
        public async Task Should_call_action(ActionMethod actionMethod)
        {
            var requestGraph = RequestGraph.CreateFor(actionMethod);
            var requestContext = requestGraph.GetRequestContext();

            requestGraph.AddResponseWriter1(c => c.Response.ToString().CreateTextResponse().ToTaskResult());
            requestGraph.AddRequestBinder1(c => SetArguments(c, (a, p) => a[p.Position] = $"value{p.Position}"));
            requestGraph.AddRequestBinder2(c => SetArguments(c, (a, p) => a[p.Position] += $"-{p.Position}"));

            var invoker = new ActionInvoker(requestContext, requestGraph.RequestBinders, 
                requestGraph.ResponseWriters, requestGraph.Configuration);
            var handler = Substitute.For<IHandler>();

            handler.Response().ReturnsForAnyArgs(x => "response");
            handler.ResponseAsync().ReturnsForAnyArgs(x => "response".ToTaskResult());
            handler.ParamsAndResponse(null).ReturnsForAnyArgs(x => "response");
            handler.ParamsAndResponseAsync(null).ReturnsForAnyArgs(x => "response".ToTaskResult());

            var response = await invoker.Invoke(handler);

            var expectedArguments = actionMethod.Method.Parameters.Select(p =>
                $"value{p.Position}-{p.Position}").Cast<object>().ToArray();

            await actionMethod.Invoke(handler.Received(1), expectedArguments);

            if (requestContext.Route.HasResponse)
            {
                response.StatusCode.ShouldEqual(HttpStatusCode.OK);
                var responseText = await response.Content.ReadAsStringAsync();
                responseText.ShouldEqual("response");
            }
            else response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
        }

        private static Task SetArguments(RequestBinderContext context, Action<object[], ParameterDescriptor> set)
        {
            context.RequestContext.Action.Method.Parameters.ForEach(pi => set(context.ActionArguments, pi));
            return Task.CompletedTask;
        }

        [Test]
        public async Task Should_not_run_binders_where_instance_does_not_apply()
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.Params(null, null));
            var requestContext = requestGraph.GetRequestContext();

            requestGraph.AddRequestBinder1(c => SetArguments(c, (a, p) => a[0] = "binder1"));
            requestGraph.AddRequestBinder2(c => SetArguments(c, (a, p) => a[1] += "binder2"),
                instanceAppliesTo: x => false);

            var invoker = new ActionInvoker(requestContext, requestGraph.RequestBinders, 
                requestGraph.ResponseWriters, requestGraph.Configuration);
            var handler = Substitute.For<IHandler>();

            await invoker.Invoke(handler);

            var expectedArguments = new object[] { "binder1", null };

            await requestGraph.ActionMethod.Invoke(handler.Received(1), expectedArguments);

            requestGraph.RequestBinder1.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestBinder1.AppliesToContext.RequestContext.ShouldEqual(requestContext);
            requestGraph.RequestBinder1.AppliesToContext.ActionArguments.ShouldEqual(expectedArguments);
            requestGraph.RequestBinder1.BindCalled.ShouldBeTrue();
            requestGraph.RequestBinder1.BindContext.RequestContext.ShouldEqual(requestContext);
            requestGraph.RequestBinder1.BindContext.ActionArguments.ShouldEqual(expectedArguments);

            requestGraph.RequestBinder2.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestBinder2.AppliesToContext.RequestContext.ShouldEqual(requestContext);
            requestGraph.RequestBinder2.AppliesToContext.ActionArguments.ShouldEqual(expectedArguments);
            requestGraph.RequestBinder2.BindCalled.ShouldBeFalse();
        }

        [Test]
        public async Task Should_not_run_binders_that_do_not_apply_in_configuration()
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.Params(null, null));
            var requestContext = requestGraph.GetRequestContext();

            requestGraph.AddRequestBinder1(c => SetArguments(c, (a, p) => a[0] = "binder1"));
            requestGraph.AddRequestBinder2(c => SetArguments(c, (a, p) => a[1] += "binder2"), 
                configAppliesTo: x => false);

            var invoker = new ActionInvoker(requestContext, requestGraph.RequestBinders, 
                requestGraph.ResponseWriters, requestGraph.Configuration);
            var handler = Substitute.For<IHandler>();

            await invoker.Invoke(handler);

            var expectedArguments = new object[] { "binder1", null };

            await requestGraph.ActionMethod.Invoke(handler.Received(1), expectedArguments);

            requestGraph.RequestBinder1.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestBinder1.AppliesToContext.RequestContext.ShouldEqual(requestContext);
            requestGraph.RequestBinder1.AppliesToContext.ActionArguments.ShouldEqual(expectedArguments);
            requestGraph.RequestBinder1.BindCalled.ShouldBeTrue();
            requestGraph.RequestBinder1.BindContext.RequestContext.ShouldEqual(requestContext);
            requestGraph.RequestBinder1.BindContext.ActionArguments.ShouldEqual(expectedArguments);

            requestGraph.RequestBinder2.AppliesToCalled.ShouldBeFalse();
            requestGraph.RequestBinder2.BindCalled.ShouldBeFalse();
        }

        [Test]
        public async Task Should_call_handler_with_first_writer_that_applies()
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.Response());

            requestGraph.AddResponseWriter1(x => $"{x.Response}1".CreateTextResponse().ToTaskResult(),
                instanceAppliesTo: x => false);
            requestGraph.AddResponseWriter2(x => $"{x.Response}2".CreateTextResponse().ToTaskResult());

            var invoker = new ActionInvoker(requestGraph.GetRequestContext(), 
                requestGraph.RequestBinders, requestGraph.ResponseWriters, 
                requestGraph.Configuration);
            var handler = Substitute.For<IHandler>();

            handler.Response().ReturnsForAnyArgs(x => "response");

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            var responseText = await response.Content.ReadAsStringAsync();
            responseText.ShouldEqual("response2");
        }

        [Test]
        public async Task Should_return_empty_response_if_no_writers_apply(
            [Values(null, HttpStatusCode.OK)] HttpStatusCode? statusCode)
        {
            var requestGraph = RequestGraph.CreateFor<IHandler>(x => x.Response());
            if (statusCode.HasValue) requestGraph.Configuration.DefaultStatusCode = statusCode.Value;
            var invoker = new ActionInvoker(requestGraph.GetRequestContext(), 
                requestGraph.RequestBinders, requestGraph.ResponseWriters, 
                requestGraph.Configuration);

            var handler = Substitute.For<IHandler>();
            handler.Response().ReturnsForAnyArgs(x => "response");

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(requestGraph.Configuration.DefaultStatusCode);
        }
    }
}