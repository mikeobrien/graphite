using System;
using System.IO;
using System.Net;
using System.Threading;
using NUnit.Framework;
using Should;
using TestHarness;
using Tests.Common;

namespace Tests.Acceptance
{
    [TestFixture]
    public class PerformanceTests
    {
        [Test][Ignore("Manually run")]
        public void Performance()
        {
            var guid = Guid.Parse("6e7335ea-5968-4cf5-84a2-0b8ef560a865");
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

            Should_match_result(Http.PostJson<PerfInputModel, PerfOutputModel>(graphiteUrl, inputModel), guid);
            Should_match_result(Http.PostJson<PerfInputModel, PerfOutputModel>(webapiUrl, inputModel), guid);
            Should_match_result(Http.PostJson<PerfInputModel, PerfOutputModel>(graphiteAsyncUrl, inputModel), guid);
            Should_match_result(Http.PostJson<PerfInputModel, PerfOutputModel>(webapiAsyncUrl, inputModel), guid);

            3.Times(() =>
            {
                Thread.Sleep(5000);
                var comparison = PerformanceComparison.InMilliseconds(100, 20, 40);

                var graphite = comparison.AddCase("Graphite", () => Http.PostJson<PerfInputModel, PerfOutputModel>(graphiteUrl, inputModel));
                var graphiteAsync = comparison.AddCase("Graphite Async", () => Http.PostJson<PerfInputModel, PerfOutputModel>(graphiteAsyncUrl, inputModel));
                var webapi = comparison.AddCase("Web Api", () => Http.PostJson<PerfInputModel, PerfOutputModel>(webapiUrl, inputModel));
                var webapiAsync = comparison.AddCase("Web Api Async", () => Http.PostJson<PerfInputModel, PerfOutputModel>(webapiAsyncUrl, inputModel));

                comparison.Run();

                File.AppendAllText(@"c:\temp\graphite.txt", $"{graphite.Average},{graphiteAsync.Average},{webapi.Average},{webapiAsync.Average}\r\n");
            });
        }

        private void Should_match_result(Http.Result<PerfOutputModel> outputModel, Guid guid)
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
