﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Linq;
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
        public class Model { }

        public interface IHandler
        {
            [ResponseHeader("fark", "farker")]
            void NoParamsOrResponse();
            [ResponseHeader("fark", "farker")]
            Task NoParamsOrResponseAsync();
            [ResponseHeader("fark", "farker")]
            void Params(string param);
            [ResponseHeader("fark", "farker")]
            Task ParamsAsync(string param);
            [ResponseHeader("fark", "farker")]
            void Params(string param1, string param2);
            [ResponseHeader("fark", "farker")]
            Task ParamsAsync(string param1, string param2);
            [ResponseHeader("fark", "farker")]
            string Response();
            [ResponseHeader("fark", "farker")]
            void NoResponse();
            [ResponseHeader("fark", "farker")]
            HttpResponseMessage HttpResponseMessageResponse();
            [ResponseHeader("fark", "farker")]
            Task<string> ResponseAsync();
            [ResponseHeader("fark", "farker")]
            string ParamsAndResponse(string param);
            [ResponseHeader("fark", "farker")]
            Task<string> ParamsAndResponseAsync(string param);
            [ResponseHeader("fark", "farker")]
            void Request(Model request);
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
            var requestGraph = RequestGraph
                .CreateFor(actionMethod)
                .AddDefaultResponseStatus()
                .AddDefaultResponseHeaders();
            var routeDescriptor = requestGraph.GetRouteDescriptor();
            var responseMessage = requestGraph.GetHttpResponseMessage();

            requestGraph.AddResponseWriter1(c =>
            {
                responseMessage.Content = new StringContent(c.Response.ToString());
                return responseMessage.ToTaskResult();
            });

            requestGraph.AddRequestBinder1(c => SetArguments(requestGraph.ActionMethod, 
                c, (a, p) => a[p.Position] = $"value{p.Position}"));
            requestGraph.AddRequestBinder2(c => SetArguments(requestGraph.ActionMethod, 
                c, (a, p) => a[p.Position] += $"-{p.Position}"));

            var invoker = CreateInvoker(requestGraph, responseMessage);
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
            else response.StatusCode.ShouldEqual(requestGraph
                .Configuration.DefaultNoResponseStatusCode);
            
            response.Headers.GetValues("fark").ShouldOnlyContain("farker");
        }

        private static Task<BindResult> SetArguments(ActionMethod actionMethod, 
            RequestBinderContext context, Action<object[], ParameterDescriptor> set)
        {
            actionMethod.MethodDescriptor.Parameters.ForEach(pi => set(context.ActionArguments, pi));
            return BindResult.Success().ToTaskResult();
        }

        [Test]
        public async Task Should_not_run_binders_where_instance_does_not_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.Params(null, null))
                .AddDefaultResponseStatus();

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
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.Params(null, null))
                .AddDefaultResponseStatus();

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
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.Response())
                .AddDefaultResponseStatus();

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
        public async Task Should_set_default_response_status(
            [Values(null, HttpStatusCode.Created)] HttpStatusCode? statusCode)
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.Response())
                .AddDefaultResponseStatus();
            var responseMessage = requestGraph.GetHttpResponseMessage();

            if (statusCode.HasValue) requestGraph.Configuration
                .DefaultHasResponseStatusCode = statusCode.Value;

            requestGraph.AddResponseWriter1(x => responseMessage.ToTaskResult());

            var invoker = CreateInvoker(requestGraph, responseMessage);
            var handler = Substitute.For<IHandler>();

            handler.Response().ReturnsForAnyArgs(x => "response");

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(requestGraph
                .Configuration.DefaultHasResponseStatusCode);
        }

        [Test]
        public async Task Should_set_default_no_response_status(
            [Values(null, HttpStatusCode.Created)] HttpStatusCode? statusCode)
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.NoResponse())
                .AddDefaultResponseStatus();

            if (statusCode.HasValue) requestGraph.Configuration
                .DefaultNoResponseStatusCode = statusCode.Value;

            requestGraph.AddResponseWriter1(x => null);

            var invoker = CreateInvoker(requestGraph);
            var handler = Substitute.For<IHandler>();

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(requestGraph
                .Configuration.DefaultNoResponseStatusCode);
        }

        [Test]
        public async Task Should_use_default_writer_if_set_and_no_writers_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.Response())
                .AddDefaultResponseStatus();
            
            requestGraph.AddResponseWriter1(x => $"{x.Response}1".CreateTextResponse()
                .ToTaskResult(), instanceAppliesTo: x => false);
            requestGraph.AddResponseWriter2(x => $"{x.Response}2".CreateTextResponse()
                .ToTaskResult(), instanceAppliesTo: x => false, @default:  true);

            var invoker = CreateInvoker(requestGraph);
            var handler = Substitute.For<IHandler>();

            handler.Response().ReturnsForAnyArgs(x => "response");

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(requestGraph
                .Configuration.DefaultHasResponseStatusCode);
            var responseText = await response.Content.ReadAsStringAsync();
            responseText.ShouldEqual("response2");
        }

        [Test]
        public async Task Should_directly_return_http_response_message()
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.HttpResponseMessageResponse())
                .AddDefaultResponseStatus();

            var invoker = CreateInvoker(requestGraph);
            var handler = Substitute.For<IHandler>();
            var responseMessage = new HttpResponseMessage();

            handler.HttpResponseMessageResponse().ReturnsForAnyArgs(x => responseMessage);

            var response = await invoker.Invoke(handler);

            response.ShouldEqual(responseMessage);
        }

        [Test]
        public async Task Should_return_empty_response_if_no_writers_apply(
            [Values(null, HttpStatusCode.Created)] HttpStatusCode? statusCode)
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.Response())
                .AddDefaultResponseStatus();

            if (statusCode.HasValue) requestGraph.Configuration
                .DefaultNoWriterStatusCode = statusCode.Value;
            var invoker = CreateInvoker(requestGraph);

            var handler = Substitute.For<IHandler>();
            handler.Response().ReturnsForAnyArgs(x => "response");

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(requestGraph
                .Configuration.DefaultNoWriterStatusCode);
        }

        [Test]
        public async Task Should_return_reader_failure(
            [Values(null, HttpStatusCode.Created)] HttpStatusCode? statusCode)
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.Request(null))
                    .AddDefaultResponseStatus()
                    .WithContentType(MimeTypes.ApplicationJson)
                    .WithRequestParameter("request")
                    .WithRequestData("{");

            requestGraph.AddReaderBinder(requestGraph.GetJsonReader());

            if (statusCode.HasValue) requestGraph.Configuration
                .DefaultBindingFailureStatusCode = statusCode.Value;
            var invoker = CreateInvoker(requestGraph);

            var handler = Substitute.For<IHandler>();
            handler.Response().ReturnsForAnyArgs(x => "response");

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(requestGraph
                .Configuration.DefaultBindingFailureStatusCode);
        }

        [Test]
        public async Task Should_return_no_reader_status_when_there_is_a_request_body_and_no_reader(
            [Values(null, HttpStatusCode.Created)] HttpStatusCode? statusCode)
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.Request(null))
                    .AddDefaultResponseStatus()
                    .WithContentType("fark/farker")
                    .WithRequestParameter("request")
                    .WithRequestData("fark")
                    .AddReaderBinder();

            if (statusCode.HasValue) requestGraph.Configuration
                .DefaultNoReaderStatusCode = statusCode.Value;
            var invoker = CreateInvoker(requestGraph);

            var handler = Substitute.For<IHandler>();
            handler.Response().ReturnsForAnyArgs(x => "response");

            var response = await invoker.Invoke(handler);

            response.StatusCode.ShouldEqual(requestGraph
                .Configuration.DefaultNoReaderStatusCode);
        }

        private ActionInvoker CreateInvoker(RequestGraph requestGraph, 
            HttpResponseMessage responseMessage = null)
        {
            return new ActionInvoker(requestGraph.GetActionDescriptor(),
                requestGraph.RequestBinders, requestGraph.ResponseWriters,
                requestGraph.ResponseStatus, requestGraph.ResponseHeaders, 
                responseMessage ?? requestGraph.GetHttpResponseMessage());
        }
    }
}