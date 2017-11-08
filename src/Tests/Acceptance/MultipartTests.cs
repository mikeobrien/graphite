using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using Graphite.Extensions;
using Graphite.Http;
using NUnit.Framework;
using Should;
using TestHarness;
using TestHarness.Multipart;
using Tests.Common;

namespace Tests.Acceptance
{
    [TestFixture]
    public class MultipartTests
    {
        private const string BaseUrl = "Multipart/";

        [Test]
        public void Should_post_single_form_value_to_action_parameter([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<MultipartTestHandler
                .OutputModel>($"{BaseUrl}SingleValueToParameter", x => x
                    .AddTextFormData("value", "fark") );

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value1.ShouldEqual("fark");
        }

        [Test]
        public void Should_post_multiple_form_values_to_action_parameter([Values(Host.Owin, Host.IISExpress)] Host host,
            [Values("MultipleValuesToParameters", "MultipleValuesToModel")] string url)
        {
            var result = Http.ForHost(host).PostMultipartForm<MultipartTestHandler
                .OutputModel>($"{BaseUrl}{url}", x => x
                    .AddTextFormData("value1", "fark")
                    .AddTextFormData("value2", "5"));

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value1.ShouldEqual("fark");
            result.Data.Value2.ShouldEqual(5);
        }

        [Test]
        public void Should_post_json_to_model([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<MultipartTestHandler
                .OutputModel>($"{BaseUrl}MultipleValuesToModel", x => x
                .AddTextFormData("value", "{\"value1\":\"fark\",\"value2\":5}", 
                    contentType: MimeTypes.ApplicationJson));

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value1.ShouldEqual("fark");
            result.Data.Value2.ShouldEqual(5);
        }

        [Test]
        public void Should_post_json_to_model_and_values_to_params([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<MultipartTestHandler
                .OutputModel>($"{BaseUrl}MultipleValuesToModelAndParams", x => x
                .AddTextFormData("value", "{\"value1\":\"fark\",\"value2\":5}",
                    contentType: MimeTypes.ApplicationJson)
                .AddTextFormData("value3", "farker")
                .AddTextFormData("value4", "6"));

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value1.ShouldEqual("fark");
            result.Data.Value2.ShouldEqual(5);
            result.Data.Value3.ShouldEqual("farker");
            result.Data.Value4.ShouldEqual(6);
        }

        [Test]
        public void Should_post_json_and_values_to_model([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<MultipartTestHandler
                .OutputModel>($"{BaseUrl}MultipleValuesToModel", x => x
                .AddTextFormData("value", "{\"value1\":\"fark\"}",
                    contentType: MimeTypes.ApplicationJson)
                .AddTextFormData("value2", "5"));

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value1.ShouldEqual("fark");
            result.Data.Value2.ShouldEqual(5);
        }

        [Test]
        public void Should_post_single_file_to_param([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<List<MultipartTestHandler
                .InputBodyModel>>($"{BaseUrl}StreamsToParam", x => x
                .AddTextFormData("somefile", "fark", "filename.txt", MimeTypes.TextHtml));

            result.Status.ShouldEqual(HttpStatusCode.OK);

            result.Data.Count.ShouldEqual(1);

            var file = result.Data.First();

            file.Name.ShouldEqual("somefile");
            file.Filename.ShouldEqual("filename.txt");
            file.Data.ShouldEqual("fark");
            file.ContentType.ShouldEqual(MimeTypes.TextHtml);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();
        }

        [Test]
        public void Should_post_multiple_files_to_param([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<List<MultipartTestHandler
                .InputBodyModel>>($"{BaseUrl}StreamsToParam", x => x
                .AddTextFormData("somefile", "fark", "filename.txt", MimeTypes.TextHtml)
                .AddTextFormData("anotherfile", "farker", "filename.png", MimeTypes.ImagePng));

            result.Status.ShouldEqual(HttpStatusCode.OK);

            result.Data.Count.ShouldEqual(2);

            var file = result.Data.First();

            file.Name.ShouldEqual("somefile");
            file.Filename.ShouldEqual("filename.txt");
            file.Data.ShouldEqual("fark");
            file.ContentType.ShouldEqual(MimeTypes.TextHtml);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();

            file = result.Data.Second();

            file.Name.ShouldEqual("anotherfile");
            file.Filename.ShouldEqual("filename.png");
            file.Data.ShouldEqual("farker");
            file.ContentType.ShouldEqual(MimeTypes.ImagePng);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();
        }

        [Test]
        public void Should_post_multiple_files_and_values_to_params([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<MultipartTestHandler
                .InputBodyResponse>($"{BaseUrl}StreamsAndValuesToParams", x => x
                .AddTextFormData("value1", "fark")
                .AddTextFormData("value2", "5")
                .AddTextFormData("somefile", "fark", "filename.txt", MimeTypes.TextHtml)
                .AddTextFormData("anotherfile", "farker", "filename.png", MimeTypes.ImagePng));

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value1.ShouldEqual("fark");
            result.Data.Value2.ShouldEqual(5);

            result.Data.Files.Count.ShouldEqual(2);

            var file = result.Data.Files.First();

            file.Name.ShouldEqual("somefile");
            file.Filename.ShouldEqual("filename.txt");
            file.Data.ShouldEqual("fark");
            file.ContentType.ShouldEqual(MimeTypes.TextHtml);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();

            file = result.Data.Files.Second();

            file.Name.ShouldEqual("anotherfile");
            file.Filename.ShouldEqual("filename.png");
            file.Data.ShouldEqual("farker");
            file.ContentType.ShouldEqual(MimeTypes.ImagePng);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();
        }

        [Test]
        public void Should_post_multiple_files_to_model([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<MultipartTestHandler
                .InputBodyResponse>($"{BaseUrl}InputStreamsAndValuesToModel", x => x
                .AddTextFormData("somefile", "fark", "filename.txt", MimeTypes.TextHtml)
                .AddTextFormData("anotherfile", "farker", "filename.png", MimeTypes.ImagePng));

            result.Status.ShouldEqual(HttpStatusCode.OK);

            result.Data.Files.Count.ShouldEqual(2);

            var file = result.Data.Files.First();

            file.Name.ShouldEqual("somefile");
            file.Filename.ShouldEqual("filename.txt");
            file.Data.ShouldEqual("fark");
            file.ContentType.ShouldEqual(MimeTypes.TextHtml);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();

            file = result.Data.Files.Second();

            file.Name.ShouldEqual("anotherfile");
            file.Filename.ShouldEqual("filename.png");
            file.Data.ShouldEqual("farker");
            file.ContentType.ShouldEqual(MimeTypes.ImagePng);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();
        }

        [Test]
        public void Should_post_multiple_files_to_enumerable_streams_on_model(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<MultipartTestHandler
                .InputBodyResponse>($"{BaseUrl}OnlyStreamsAndValuesToModel", x => x
                .AddTextFormData("value1", "fark")
                .AddTextFormData("value2", "5")
                .AddTextFormData("somefile", "fark", "filename.txt", MimeTypes.TextHtml)
                .AddTextFormData("anotherfile", "farker", "filename.png", MimeTypes.ImagePng));

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value1.ShouldEqual("fark");
            result.Data.Value2.ShouldEqual(5);

            result.Data.Files.Count.ShouldEqual(2);

            var file = result.Data.Files.First();

            file.Data.ShouldEqual("fark");
            file.Name.ShouldBeNull();
            file.Filename.ShouldBeNull();
            file.ContentType.ShouldBeNull();
            file.Encoding.ShouldBeNull();
            file.Length.ShouldBeNull();

            file = result.Data.Files.Second();

            file.Data.ShouldEqual("farker");
            file.Name.ShouldBeNull();
            file.Filename.ShouldBeNull();
            file.ContentType.ShouldBeNull();
            file.Encoding.ShouldBeNull();
            file.Length.ShouldBeNull();
        }

        [Test]
        public void Should_post_multiple_files_and_values_to_model([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<MultipartTestHandler
                .InputBodyResponse>($"{BaseUrl}InputStreamsAndValuesToModel", x => x
                .AddTextFormData("value1", "fark")
                .AddTextFormData("value2", "5")
                .AddTextFormData("somefile", "fark", "filename.txt", MimeTypes.TextHtml)
                .AddTextFormData("anotherfile", "farker", "filename.png", MimeTypes.ImagePng));

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value1.ShouldEqual("fark");
            result.Data.Value2.ShouldEqual(5);

            result.Data.Files.Count.ShouldEqual(2);

            var file = result.Data.Files.First();

            file.Name.ShouldEqual("somefile");
            file.Filename.ShouldEqual("filename.txt");
            file.Data.ShouldEqual("fark");
            file.ContentType.ShouldEqual(MimeTypes.TextHtml);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();

            file = result.Data.Files.Second();

            file.Name.ShouldEqual("anotherfile");
            file.Filename.ShouldEqual("filename.png");
            file.Data.ShouldEqual("farker");
            file.ContentType.ShouldEqual(MimeTypes.ImagePng);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();
        }

        [Test]
        public void Should_post_multiple_files_and_json_and_values_to_model([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostMultipartForm<MultipartTestHandler
                .InputBodyResponse>($"{BaseUrl}InputStreamsAndValuesToModel", x => x
                .AddTextFormData("value", "{\"value1\":\"fark\",\"value2\":5}",
                    contentType: MimeTypes.ApplicationJson)
                .AddTextFormData("value3", "farker")
                .AddTextFormData("value4", "6")
                .AddTextFormData("somefile", "fark", "filename.txt", MimeTypes.TextHtml)
                .AddTextFormData("anotherfile", "farker", "filename.png", MimeTypes.ImagePng));

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value1.ShouldEqual("fark");
            result.Data.Value2.ShouldEqual(5);

            result.Data.Files.Count.ShouldEqual(2);

            var file = result.Data.Files.First();

            file.Name.ShouldEqual("somefile");
            file.Filename.ShouldEqual("filename.txt");
            file.Data.ShouldEqual("fark");
            file.ContentType.ShouldEqual(MimeTypes.TextHtml);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();

            file = result.Data.Files.Second();

            file.Name.ShouldEqual("anotherfile");
            file.Filename.ShouldEqual("filename.png");
            file.Data.ShouldEqual("farker");
            file.ContentType.ShouldEqual(MimeTypes.ImagePng);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldBeNull();
        }

        [Test]
        public void Should_post_large_files([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var stopwatch = new Stopwatch();
            var stream1 = new DummyStream(100.MB());
            var stream2 = new DummyStream(200.MB());

            var stream1Length = stream1.Length;
            var stream2Length = stream2.Length;

            stopwatch.Start();

            var result = Http.ForHost(host).PostMultipartForm<List<MultipartTestHandler
                .InputBodyModel>>($"{BaseUrl}LargeStreams", x => x
                //.InputBodyModel>>("webapi/multipart", x => x
                .AddStreamFormData("somefile", stream1, "filename.txt", MimeTypes.TextHtml)
                .AddStreamFormData("anotherfile", stream2, "filename.png", MimeTypes.ImagePng));

            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);

            result.Status.ShouldEqual(HttpStatusCode.OK);

            result.Data.Count.ShouldEqual(2);

            var file = result.Data.First();

            Console.WriteLine($"Checksum: {file.Data}");
            Console.WriteLine($"Length: {file.Length}");

            file.Name.ShouldEqual("somefile");
            file.Filename.ShouldEqual("filename.txt");
            file.Data.ShouldEqual(stream1.Checksum.ToString());
            file.ContentType.ShouldEqual(MimeTypes.TextHtml);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldEqual(stream1Length);

            file = result.Data.Second();

            Console.WriteLine($"Checksum: {file.Data}");
            Console.WriteLine($"Length: {file.Length}");

            file.Name.ShouldEqual("anotherfile");
            file.Filename.ShouldEqual("filename.png");
            file.Data.ShouldEqual(stream2.Checksum.ToString());
            file.ContentType.ShouldEqual(MimeTypes.ImagePng);
            file.Encoding.ShouldBeEmpty();
            file.Length.ShouldEqual(stream2Length);
        }

        [Test]
        public void Should_return_400_when_multipart_read_error_occurs(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostString(
                $"{BaseUrl}SingleValueToParameter", 
                "--some-boundary\r\n" +
                "Content-Disposition: form-data; name=value\r\n" +
                "\r\n" +
                "5" +
                "--some-boundary--", 
                requestHeaders: x => x.
                    Accept.Add(new MediaTypeWithQualityHeaderValue(MimeTypes.ApplicationJson)),
                contentHeaders: x =>
                {
                    x.ContentType = new MediaTypeWithQualityHeaderValue(MimeTypes.MultipartFormData)
                    {
                        Parameters =
                        {
                            new NameValueHeaderValue("boundary", "\"some-boundary\"")
                        }
                    };
                });
            
            result.Status.ShouldEqual(HttpStatusCode.BadRequest);
            result.ReasonPhrase.ShouldEqual("Boundary not preceeded by CRLF.");
        }
    }
}
