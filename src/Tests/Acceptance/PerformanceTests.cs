using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Should;
using TestHarness;
using Tests.Common;
using Handler = TestHarness.Handler;
using WebClient = Tests.Common.WebClient;

namespace Tests.Acceptance
{
    [TestFixture]
    public class PerformanceTests
    {
        [Test][Ignore("Manually run")]
        public void PerformanceComparison()
        {
            var iterations = 100;

            var graphite = new ConcurrentBag<long>();
            var graphiteAsync = new ConcurrentBag<long>();
            var webapi = new ConcurrentBag<long>();
            var webapiAsync = new ConcurrentBag<long>();
            var guid = Guid.NewGuid();
            var url = $"performancetests/{{0}}/url1/{guid}/5?query1=query1&query2={guid}&query3=5";
            var urlAsync = $"performancetests/{{0}}/async/url1/{guid}/5?query1=query1&query2={guid}&query3=5";
            var graphiteUrl = string.Format(url, "graphite");
            var graphiteAsyncUrl = string.Format(urlAsync, "graphite");
            var webapiUrl = string.Format(url, "webapi");
            var webapiAsyncUrl = string.Format(urlAsync, "webapi");
            var inputModel = new PerfInputModel
            {
                Value1 = "value1",
                Value2 = "value2",
                Value3 = "value3"
            };

            10.TimesParallel(() =>
            {
                Should_match_result(WebClient.PostJson<PerfInputModel, PerfOutputModel>(graphiteUrl, inputModel), guid);
                Should_match_result(WebClient.PostJson<PerfInputModel, PerfOutputModel>(webapiUrl, inputModel), guid);
                Should_match_result(WebClient.PostJson<PerfInputModel, PerfOutputModel>(graphiteAsyncUrl, inputModel), guid);
                Should_match_result(WebClient.PostJson<PerfInputModel, PerfOutputModel>(webapiAsyncUrl, inputModel), guid);
            });

            iterations.TimesParallel(() =>
            {
                graphite.Add(graphiteUrl.ElapsedMilliseconds(x => WebClient.PostJson<PerfInputModel, PerfOutputModel>(x, inputModel)));
                webapi.Add(webapiUrl.ElapsedMilliseconds(x => WebClient.PostJson<PerfInputModel, Handler.OutputModel>(x, inputModel)));
                graphiteAsync.Add(graphiteUrl.ElapsedMilliseconds(x => WebClient.PostJson<PerfInputModel, PerfOutputModel>(x, inputModel)));
                webapiAsync.Add(webapiUrl.ElapsedMilliseconds(x => WebClient.PostJson<PerfInputModel, Handler.OutputModel>(x, inputModel)));
            });

            Console.WriteLine($"Graphite :      {graphite.Average()}ms");
            Console.WriteLine($"Graphite Async: {graphiteAsync.Average()}ms");
            Console.WriteLine($"Web Api:        {webapi.Average()}ms");
            Console.WriteLine($"Web Api Async:  {webapiAsync.Average()}ms");
        }

        private void Should_match_result(WebClient.Result<PerfOutputModel> outputModel, Guid guid)
        {
            outputModel.Status.ShouldEqual(HttpStatusCode.OK);
            outputModel.Data.ShouldNotBeNull();

            outputModel.Data.Value1.ShouldEqual("value1");
            outputModel.Data.Value2.ShouldEqual("value2");
            outputModel.Data.Value3.ShouldEqual("value3");
            outputModel.Data.Query1.ShouldEqual("query1");
            outputModel.Data.Query2.ShouldEqual(guid);
            outputModel.Data.Query3.ShouldEqual(5);
            outputModel.Data.Url1.ShouldEqual("url1");
            outputModel.Data.Url2.ShouldEqual(guid);
            outputModel.Data.Url3.ShouldEqual(5);
        }
    }
}
