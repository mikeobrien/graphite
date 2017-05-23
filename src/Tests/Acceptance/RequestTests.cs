using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Graphite.Http;
using NUnit.Framework;
using Should;
using TestHarness;
using Tests.Common;
using WebClient = Tests.Common.WebClient;

namespace Tests.Acceptance
{
    [TestFixture]
    public class RequestTests
    {
        [Test]
        public void Should_get_with_no_response()
        {
            var result = WebClient.GetString("WithNoResponse");

            result.Status.ShouldEqual(HttpStatusCode.NoContent);
            result.Data.ShouldBeEmpty();
        }

        [Test]
        public void Should_get_with_response()
        {
            var result = WebClient.GetJson<Handler.OutputModel>("WithResponse");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationJson);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_get_with_response_querystring_and_url_parameters(
            [Values("", "&query1=query1b&query2=7")] string querystring)
        {
            var result = WebClient.GetJson<Handler.OutputModel>(
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
            var result = WebClient.GetJson<Handler.OutputModel>(
                $"WithNullableQueryParams{querystring}");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.NullableQuery.ShouldEqual(value);
        }

        [Test]
        public void Should_get_with_response_multiple_querystring_parameters()
        {
            var result = WebClient.GetJson<Handler.OutputModel>(
                "WithMultiQueryParams?query1=querya&query2=6&query1=queryb&query2=7");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.MultiQuery1.ShouldOnlyContain("querya", "queryb");
            result.Data.MultiQuery2.ShouldOnlyContain(6, 7);
        }

        [Test]
        public void Should_get_with_wildcard_parameters()
        {
            var result = WebClient.GetJson<Handler.OutputModel>(
                "WithSingleValueWildcard/1/2/3/4/5");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.UrlWildcard.ShouldEqual("1/2/3/4/5");
        }

        [Test]
        public void Should_get_multi_wildcard_parameters(
            [Values("WithParamsWildcard", "WithAttriubteWildcard")] string url)
        {
            var result = WebClient.GetJson<Handler.OutputModel>(
                $"{url}/1/2/3/4/5");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.UrlMultiWildcard.ShouldOnlyContain(1, 2, 3, 4, 5);
        }

        [Test]
        public void Should_get_xml()
        {
            var result = WebClient.GetXml<Handler.OutputModel>("WithResponse");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationXml);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_return_400_when_there_is_a_param_parsing_error()
        {
            var result = WebClient.GetJson<Handler.OutputModel>(
                "WithQueryParam?query=fark");

            result.Status.ShouldEqual(HttpStatusCode.BadRequest);
            result.StatusText.ShouldContain("query");
            result.StatusText.ShouldContain("fark");
        }

        [Test]
        public void Should_post_with_no_response()
        {
            WebClient.PostJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_post_with_response(
            [Values("", "&query1=query1b&query2=7")] string querystring)
        {
            var result = WebClient.PostJson<Handler.InputModel, Handler.OutputModel>(
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
        public void Should_post_json_to_action_parameters()
        {
            var result = WebClient.PostJson<Handler.InputModel, Handler.OutputModel>("ToParameters",
                new Handler.InputModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationJson);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_post_xml_to_action_parameters()
        {
            var result = WebClient.PostXml<Handler.InputModel, Handler.OutputModel>("ToParameters",
                new Handler.InputModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationXml);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_post_and_return_xml()
        {
            var result = WebClient.PostXml<Handler.InputModel, Handler.OutputModel>("WithResponse",
                new Handler.InputModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationXml);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_post_and_return_string()
        {
            var result = WebClient.PostString("String", "fark");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.TextPlain);
            result.Data.ShouldEqual("fark");
        }

        [Test]
        public void Should_post_form_url_encoded_to_params(
            [Values("", "&query1=query1b&query2=7")] string querystring)
        {
            var result = WebClient.PostForm<Handler.OutputModel>(
                "WithQueryParams", $"query1=query1&query2=6{querystring}");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Query1.ShouldEqual("query1");
            result.Data.Query2.ShouldEqual(6);
        }

        [Test]
        public void Should_post_form_url_encoded_multi_to_params()
        {
            var result = WebClient.PostForm<Handler.OutputModel>("WithMultiQueryParams", 
                "query1=querya&query2=6&query1=queryb&query2=7");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.MultiQuery1.ShouldOnlyContain("querya", "queryb");
            result.Data.MultiQuery2.ShouldOnlyContain(6, 7);
        }

        [Test]
        public void Should_post_form_url_encoded_to_model()
        {
            var result = WebClient.PostForm<Handler.OutputModel>("WithResponse",
                "form1=forma&form2=6&form1=formb&form2=7");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Form1.ShouldEqual("forma");
            result.Data.Form2.ShouldEqual(6);
        }

        [Test]
        public void Should_post_form_url_encoded_multi_params_to_model()
        {
            var result = WebClient.PostForm<Handler.OutputModel>("WithResponse",
                "multiform1=forma&multiform2=6&multiform1=formb&multiform2=7");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.MultiForm1.ShouldOnlyContain("forma", "formb");
            result.Data.MultiForm2.ShouldOnlyContain(6, 7);
        }

        [Test]
        public void Should_put_with_no_response()
        {
            WebClient.PutJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_put_with_response([Values("", 
            "&query1=query1b&query2=7")] string querystring)
        {
            var result = WebClient.PutJson<Handler.InputModel, Handler.OutputModel>(
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
            WebClient.PatchJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_patch_with_response([Values("", 
            "&query1=query1b&query2=7")] string querystring)
        {
            var result = WebClient.PatchJson<Handler.InputModel, Handler.OutputModel>(
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
            WebClient.DeleteJson("WithNoResponse", new Handler.InputModel())
                .Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_delete_with_response([Values("", 
            "&query1=query1b&query2=7")] string querystring)
        {
            var result = WebClient.DeleteJson<Handler.InputModel, Handler.OutputModel>(
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
        public void Should_create_url_alias()
        {
            var result = WebClient.GetJson<Handler.OutputModel>("SomeAlias");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_include_path_in_namespace()
        {
            var result = WebClient.GetJson<Handler.OutputModel>("SomeNamespace/SomeMethod");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_get_stream([Values("Stream1", "Stream2")] string url)
        {
            var result = WebClient.GetStream(url, data =>
            {
                data.ReadAllText().ShouldEqual("fark");
            });
            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Filename.ShouldEqual("weddingsinger.mp4");
            result.ContentType.ShouldEqual("application/video");
        }

        [TestCase("Stream1", null, null, null)]
        [TestCase("Stream2", "application/video", "weddingsinger.mp4", 4)]
        public void Should_post_stream(string url, string mimetype, string filename, int? length)
        {
            var result = WebClient.PostStream<Handler.StreamInfoModel>(url,
                new MemoryStream(Encoding.UTF8.GetBytes("fark")), 
                    "application/video", "weddingsinger.mp4");

            result.Data.Filename.ShouldEqual(filename);
            result.Data.MimeType.ShouldEqual(mimetype);
            result.Data.Length.ShouldEqual(length);
        }

        [Test]
        public void Should_redirect([Values("Redirect",
            "RedirectModel?redirect=true")] string url)
        {
            var result = WebClient.GetString(url);

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.ShouldContain("google");
        }

        [Test]
        public void Should_not_redirect_from_model_when_no_redirect_specified()
        {
            var result = WebClient.GetJson<Handler.RedirectModel>("RedirectModel?redirect=false");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("value");
        }

        [Test]
        public void Should_bind_cookies()
        {
            var result = WebClient.GetJson<Handler.BindingOutputModel>("WithCookies",
                new Dictionary<string, string>
                {
                    { "cookie1", "value1" },
                    { "cookie2", "value2" }
                });

            result.Status.ShouldEqual(HttpStatusCode.OK);

            result.Data.Param.ShouldBeNull();
            result.Data.ParamByName.ShouldEqual("value1");
            result.Data.ParamByAttribute.ShouldEqual("value2");
        }

        [Test]
        public void Should_bind_headers()
        {
            var result = WebClient.GetJson<Handler.BindingOutputModel>("WithHeaders",
                headers: new Dictionary<string, string>
                {
                    { "header1", "value1" },
                    { "header2", "value2" }
                });

            result.Status.ShouldEqual(HttpStatusCode.OK);

            result.Data.Param.ShouldBeNull();
            result.Data.ParamByName.ShouldEqual("value1");
            result.Data.ParamByAttribute.ShouldEqual("value2");
        }

        [Test]
        public void Should_bind_request_info()
        {
            var result = WebClient.GetJson<Handler.BindingOutputModel>("WithRequestInfo");

            result.Status.ShouldEqual(HttpStatusCode.OK);

            result.Data.Param.ShouldBeNull();
            result.Data.ParamByName.ShouldEqual("::1");
            result.Data.ParamByAttribute.ShouldEqual("HTTP/1.1");
            result.Data.ParamByType.ShouldBeTrue();
        }

        [Test]
        public void Should_write_http_response_message()
        {
            var result = WebClient.GetJson<Handler.BindingOutputModel>("WithResponseMessage");

            result.Status.ShouldEqual(HttpStatusCode.PaymentRequired);
        }
    }
}
