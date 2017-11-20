using System.Net;
using Graphite.Http;
using NUnit.Framework;
using Should;
using TestHarness;
using Tests.Common;

namespace Tests.Acceptance
{
    [TestFixture]
    public class RequestTests
    {
        [Test]
        public void Should_get_with_no_response(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetString("WithNoResponse");

            result.Status.ShouldEqual(HttpStatusCode.NoContent);
            result.Data.ShouldBeEmpty();
        }

        [Test]
        public void Should_get_with_response(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetJson<Handler.OutputModel>("WithResponse");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationJson);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_get_with_response_querystring_and_url_parameters(
            [Values("", "&query1=query1b&query2=7")] string querystring,
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetJson<Handler.OutputModel>(
                $"WithResponseUrlAndQueryParams/url1/segment/5?query1=query1&query2=6{querystring}");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
            result.Data.Url1.ShouldEqual("url1");
            result.Data.Url2.ShouldEqual(5);
            result.Data.Query1.ShouldEqual("query1");
            result.Data.Query2.ShouldEqual(6);
        }

        [TestCase("", null, Host.IISExpress)]
        [TestCase("?query=7", 7, Host.IISExpress)]

        [TestCase("", null, Host.Owin)]
        [TestCase("?query=7", 7, Host.Owin)]
        public void Should_handle_nullable_querystring_value(string querystring, int? value, Host host)
        {
            var result = Http.ForHost(host).GetJson<Handler.OutputModel>(
                $"WithNullableQueryParams{querystring}");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.NullableQuery.ShouldEqual(value);
        }

        [Test]
        public void Should_get_with_response_multiple_querystring_parameters(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetJson<Handler.OutputModel>(
                "WithMultiQueryParams?query1=querya&query2=6&query1=queryb&query2=7");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.MultiQuery1.ShouldOnlyContain("querya", "queryb");
            result.Data.MultiQuery2.ShouldOnlyContain(6, 7);
        }

        [Test]
        public void Should_return_400_when_there_is_a_param_parsing_error(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetJson<Handler.OutputModel>(
                "WithQueryParam?query=fark");

            result.Status.ShouldEqual(HttpStatusCode.BadRequest);
            result.ReasonPhrase.ShouldContain("query");
            result.ReasonPhrase.ShouldContain("fark");
        }

        [Test]
        public void Should_get_with_wildcard_parameters(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetJson<Handler.WildcardModel>(
                "WithSingleValueWildcard/1/2/3/4/5");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Single.ShouldEqual("1/2/3/4/5");
        }

        [Test]
        public void Should_get_multi_wildcard_parameters(
            [Values("WithParamsWildcard", "WithAttriubteWildcard")] string url,
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetJson<Handler.WildcardModel>(
                $"{url}/1/2/3/4/5");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Multi.ShouldOnlyContain(1, 2, 3, 4, 5);
        }

        [Test]
        public void Should_post_with_no_response(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            Http.ForHost(host).PostJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_post_with_response(
            [Values("", "&query1=query1b&query2=7")] string querystring,
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostJson<Handler.InputModel, Handler.OutputModel>(
                $"WithResponseUrlAndQueryParams/url1/segment/5?query1=query1&query2=6{querystring}",
                new Handler.InputModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationJson);
            result.Data.Value.ShouldEqual("fark");
            result.Data.Url1.ShouldEqual("url1");
            result.Data.Url2.ShouldEqual(5);
            result.Data.Query1.ShouldEqual("query1");
            result.Data.Query2.ShouldEqual(6);
        }

        [Test]
        public void Should_put_with_no_response(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            Http.ForHost(host).PutJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_put_with_response(
            [Values("", "&query1=query1b&query2=7")] string querystring,
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PutJson<Handler.InputModel, Handler.OutputModel>(
                $"WithResponseUrlAndQueryParams/url1/segment/5?query1=query1&query2=6{querystring}",
                new Handler.InputModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
            result.Data.Url1.ShouldEqual("url1");
            result.Data.Url2.ShouldEqual(5);
            result.Data.Query1.ShouldEqual("query1");
            result.Data.Query2.ShouldEqual(6);
        }

        [Test]
        public void Should_patch_with_no_response(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            Http.ForHost(host).PatchJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_patch_with_response(
            [Values("", "&query1=query1b&query2=7")] string querystring,
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PatchJson<Handler.InputModel, Handler.OutputModel>(
                $"WithResponseUrlAndQueryParams/url1/segment/5?query1=query1&query2=6{querystring}",
                new Handler.InputModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
            result.Data.Url1.ShouldEqual("url1");
            result.Data.Url2.ShouldEqual(5);
            result.Data.Query1.ShouldEqual("query1");
            result.Data.Query2.ShouldEqual(6);
        }

        [Test]
        public void Should_delete_with_no_response(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            Http.ForHost(host).DeleteJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_delete_with_response(
            [Values("", "&query1=query1b&query2=7")] string querystring,
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).DeleteJson<Handler.InputModel, Handler.OutputModel>(
                $"WithResponseUrlAndQueryParams/url1/segment/5?query1=query1&query2=6{querystring}",
                new Handler.InputModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
            result.Data.Url1.ShouldEqual("url1");
            result.Data.Url2.ShouldEqual(5);
            result.Data.Query1.ShouldEqual("query1");
            result.Data.Query2.ShouldEqual(6);
        }
    }
}
