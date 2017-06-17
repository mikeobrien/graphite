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
        public void Should_get_with_no_response()
        {
            var result = Http.GetString("WithNoResponse");

            result.Status.ShouldEqual(HttpStatusCode.NoContent);
            result.Data.ShouldBeEmpty();
        }

        [Test]
        public void Should_get_with_response()
        {
            var result = Http.GetJson<Handler.OutputModel>("WithResponse");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationJson);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_get_with_response_querystring_and_url_parameters(
            [Values("", "&query1=query1b&query2=7")] string querystring)
        {
            var result = Http.GetJson<Handler.OutputModel>(
                $"WithResponseUrlAndQueryParams/url1/segment/5?query1=query1&query2=6{querystring}");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
            result.Data.Url1.ShouldEqual("url1");
            result.Data.Url2.ShouldEqual(5);
            result.Data.Query1.ShouldEqual("query1");
            result.Data.Query2.ShouldEqual(6);
        }

        [TestCase("", null)]
        [TestCase("?query=7", 7)]
        public void Should_handle_nullable_querystring_value(string querystring, int? value)
        {
            var result = Http.GetJson<Handler.OutputModel>(
                $"WithNullableQueryParams{querystring}");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.NullableQuery.ShouldEqual(value);
        }

        [Test]
        public void Should_get_with_response_multiple_querystring_parameters()
        {
            var result = Http.GetJson<Handler.OutputModel>(
                "WithMultiQueryParams?query1=querya&query2=6&query1=queryb&query2=7");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.MultiQuery1.ShouldOnlyContain("querya", "queryb");
            result.Data.MultiQuery2.ShouldOnlyContain(6, 7);
        }

        [Test]
        public void Should_return_400_when_there_is_a_param_parsing_error()
        {
            var result = Http.GetJson<Handler.OutputModel>(
                "WithQueryParam?query=fark");

            result.Status.ShouldEqual(HttpStatusCode.BadRequest);
            result.StatusText.ShouldContain("query");
            result.StatusText.ShouldContain("fark");
        }

        [Test]
        public void Should_get_with_wildcard_parameters()
        {
            var result = Http.GetJson<Handler.WildcardModel>(
                "WithSingleValueWildcard/1/2/3/4/5");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Single.ShouldEqual("1/2/3/4/5");
        }

        [Test]
        public void Should_get_multi_wildcard_parameters(
            [Values("WithParamsWildcard", "WithAttriubteWildcard")] string url)
        {
            var result = Http.GetJson<Handler.WildcardModel>(
                $"{url}/1/2/3/4/5");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Multi.ShouldOnlyContain(1, 2, 3, 4, 5);
        }

        [Test]
        public void Should_post_with_no_response()
        {
            Http.PostJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_post_with_response(
            [Values("", "&query1=query1b&query2=7")] string querystring)
        {
            var result = Http.PostJson<Handler.InputModel, Handler.OutputModel>(
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
        public void Should_put_with_no_response()
        {
            Http.PutJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_put_with_response([Values("", 
            "&query1=query1b&query2=7")] string querystring)
        {
            var result = Http.PutJson<Handler.InputModel, Handler.OutputModel>(
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
        public void Should_patch_with_no_response()
        {
            Http.PatchJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_patch_with_response([Values("", 
            "&query1=query1b&query2=7")] string querystring)
        {
            var result = Http.PatchJson<Handler.InputModel, Handler.OutputModel>(
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
        public void Should_delete_with_no_response()
        {
            Http.DeleteJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_delete_with_response([Values("", 
            "&query1=query1b&query2=7")] string querystring)
        {
            var result = Http.DeleteJson<Handler.InputModel, Handler.OutputModel>(
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
