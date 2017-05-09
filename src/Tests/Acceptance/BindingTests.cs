using System;
using System.Collections.Generic;
using System.Net;
using Graphite.Http;
using NUnit.Framework;
using Should;
using TestHarness.Binding;
using Tests.Common;
using WebClient = Tests.Common.WebClient;

namespace Tests.Acceptance
{
    [TestFixture]
    public class BindingTests
    {
        private const string BaseUrl = "Binding/";

        [Test]
        public void Should_post_json_to_action_parameters()
        {
            var result = WebClient.PostJson<BindingTestHandler.ParameterModel, 
                BindingTestHandler.ParameterModel>($"{BaseUrl}ToParameters",
                new BindingTestHandler.ParameterModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationJson);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_post_xml_to_action_parameters()
        {
            var result = WebClient.PostXml<BindingTestHandler.ParameterModel,
                BindingTestHandler.ParameterModel>($"{BaseUrl}ToParameters",
                new BindingTestHandler.ParameterModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationXml);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_post_form_url_encoded_to_params(
            [Values("", "&form1=form1b&form2=7")] string querystring)
        {
            var result = WebClient.PostForm<BindingTestHandler.FormModel>(
                $"{BaseUrl}FormToParams", $"form1=form1&form2=6{querystring}");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Form1.ShouldEqual("form1");
            result.Data.Form2.ShouldEqual(6);
        }

        [Test]
        public void Should_post_form_url_encoded_multi_to_params()
        {
            var result = WebClient.PostForm<BindingTestHandler.MultiParamFormModel>(
                $"{BaseUrl}FormToMultiParams", "form1=forma&form2=6&form1=formb&form2=7");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Form1.ShouldOnlyContain("forma", "formb");
            result.Data.Form2.ShouldOnlyContain(6, 7);
        }

        [Test]
        public void Should_post_form_url_encoded_to_model()
        {
            var result = WebClient.PostForm<BindingTestHandler.FormModel>($"{BaseUrl}FormToModel",
                "form1=forma&form2=6&form1=formb&form2=7");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Form1.ShouldEqual("forma");
            result.Data.Form2.ShouldEqual(6);
        }

        [Test]
        public void Should_post_form_url_encoded_multi_params_to_model()
        {
            var result = WebClient.PostForm<BindingTestHandler.MultiParamFormModel>(
                $"{BaseUrl}FormToModelWithMultiParams",
                "form1=forma&form2=6&form1=formb&form2=7");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Form1.ShouldOnlyContain("forma", "formb");
            result.Data.Form2.ShouldOnlyContain(6, 7);
        }

        [Test]
        public void Should_bind_cookies()
        {
            var result = WebClient.GetJson<BindingTestHandler.BindingModel>($"{BaseUrl}WithCookies",
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
            var result = WebClient.GetJson<BindingTestHandler.BindingModel>($"{BaseUrl}WithHeaders",
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
            var result = WebClient.GetJson<BindingTestHandler.BindingModel>(
                $"{BaseUrl}WithRequestInfo");

            result.Status.ShouldEqual(HttpStatusCode.OK);

            result.Data.Param.ShouldBeNull();
            result.Data.ParamByName.ShouldEqual("::1");
            result.Data.ParamByAttribute.ShouldEqual("HTTP/1.1");
        }

        [Test]
        public void Should_bind_with_ioc_container()
        {
            var result = WebClient.GetJson<BindingTestHandler.ContainerBindingModel>(
                $"{BaseUrl}Container");

            result.Status.ShouldEqual(HttpStatusCode.OK);

            result.Data.Url.ShouldEqual($"{BaseUrl}Container");
        }
    }
}
