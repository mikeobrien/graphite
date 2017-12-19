using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Graphite;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Readers;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using Tests.Common;
using JsonReader = Graphite.Readers.JsonReader;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class MultipartFormBinderTests
    {
        public class Handler
        {
            public void Post() { }
        }

        [Test]
        public async Task Should_fail_when_reading_headers()
        {
            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "--some-boundary--", 
                RequestGraph
                    .CreateFor<Handler>(h => h.Post())
                    .GetActionDescriptor());

            var result = await binder.Bind(new RequestBinderContext(new object[] {}));

            result.Status.ShouldEqual(BindingStatus.Failure);
            result.ErrorMessage.ShouldNotBeEmpty();
        }

        public class ParameterHandler
        {
            public void Post(string value1, int value2) { }
        }

        [Test]
        public async Task Should_bind_values_to_action_parameters()
        {
            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-disposition: form-data; name=\"Value1\"\r\n" +
                "\r\n" +
                "fark\r\n" +
                "--some-boundary\r\n" +
                "content-disposition: form-data; name=\"Value2\"\r\n" +
                "\r\n" +
                "5\r\n" +
                "--some-boundary--", 
                RequestGraph
                    .CreateFor<ParameterHandler>(h => h.Post(null, 0))
                    .AddParameters("value1", "value2")
                    .GetActionDescriptor());

            var arguments = new object[2];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Success);
            result.ErrorMessage.ShouldBeNull();

            arguments.ShouldOnlyContain("fark", 5);
        }

        public class ModelPropertiesHandler
        {
            public class Model
            {
                public string Value1 { get; set; }
                public int Value2 { get; set; }
            }

            public void Post(Model request) { }
        }

        [Test]
        public async Task Should_bind_values_to_model_properties()
        {
            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-disposition: form-data; name=\"Value1\"\r\n" +
                "\r\n" +
                "fark\r\n" +
                "--some-boundary\r\n" +
                "content-disposition: form-data; name=\"Value2\"\r\n" +
                "\r\n" +
                "5\r\n" +
                "--some-boundary--", 
                RequestGraph
                    .CreateFor<ModelPropertiesHandler>(h => h.Post(null))
                    .WithRequestParameter("request")
                    .GetActionDescriptor());

            var arguments = new object[1];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Success);
            result.ErrorMessage.ShouldBeNull();

            var model = arguments[0] as ModelPropertiesHandler.Model;

            model.ShouldNotBeNull();
            model.Value1.ShouldEqual("fark");
            model.Value2.ShouldEqual(5);
        }

        public class ParametersAndModelPropertiesHandler
        {
            public class Model
            {
                public string Value1 { get; set; }
                public int Value2 { get; set; }
            }

            public void Post(Model request, string value1, int value2) { }
        }

        [Test]
        public async Task Should_bind_values_action_parameters_and_model_properties()
        {
            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-disposition: form-data; name=\"Value1\"\r\n" +
                "\r\n" +
                "fark\r\n" +
                "--some-boundary\r\n" +
                "content-disposition: form-data; name=\"Value2\"\r\n" +
                "\r\n" +
                "5\r\n" +
                "--some-boundary--", 
                RequestGraph
                    .CreateFor<ParametersAndModelPropertiesHandler>(h => h.Post(null, null, 0))
                    .WithRequestParameter("request")
                    .AddParameters("value1", "value2")
                    .GetActionDescriptor());

            var arguments = new object[3];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Success);
            result.ErrorMessage.ShouldBeNull();

            var model = arguments[0] as ParametersAndModelPropertiesHandler.Model;

            model.ShouldNotBeNull();
            model.Value1.ShouldEqual("fark");
            model.Value2.ShouldEqual(5);

            arguments[1].ShouldEqual("fark");
            arguments[2].ShouldEqual(5);
        }

        public class FailingParameterHandler
        {
            public void Post(int value) { }
        }

        [Test]
        public async Task Should_fail_binding_values_to_action_parameters()
        {
            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-disposition: form-data; name=\"Value\"\r\n" +
                "\r\n" +
                "fark\r\n" +
                "--some-boundary--", 
                RequestGraph
                    .CreateFor<FailingParameterHandler>(h => h.Post(0))
                    .AddParameters("value")
                    .GetActionDescriptor());

            var arguments = new object[1];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Failure);
            result.ErrorMessage.ShouldNotBeEmpty();
        }

        public class FailingModelPropertiesHandler
        {
            public class Model
            {
                public int Value { get; set; }
            }

            public void Post(Model request) { }
        }

        [Test]
        public async Task Should_fail_binding_values_to_model_properties()
        {
            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-disposition: form-data; name=\"Value\"\r\n" +
                "\r\n" +
                "fark\r\n" +
                "--some-boundary--", 
                RequestGraph
                    .CreateFor<FailingModelPropertiesHandler>(h => h.Post(null))
                    .WithRequestParameter("request")
                    .GetActionDescriptor());

            var arguments = new object[1];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Failure);
            result.ErrorMessage.ShouldNotBeEmpty();
        }
        
        public class ModelReaderHandler
        {
            public class Model
            {
                public string Value1 { get; set; }
                public int Value2 { get; set; }
            }

            public void Post(Model request) { }
        }

        [Test]
        public async Task Should_bind_matching_reader()
        {
            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-type: application/json\r\n" +
                "\r\n" +
                "{\"value1\":\"fark\",\"value2\":5}\r\n" +
                "--some-boundary--", 
                RequestGraph
                    .CreateFor<ModelReaderHandler>(h => h.Post(null))
                    .WithRequestParameter("request")
                    .GetActionDescriptor(),
                new JsonReader(new JsonSerializer()));

            var arguments = new object[1];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Success);
            result.ErrorMessage.ShouldBeNull();

            var model = arguments[0] as ModelReaderHandler.Model;

            model.ShouldNotBeNull();
            model.Value1.ShouldEqual("fark");
            model.Value2.ShouldEqual(5);
        }

        [Test]
        public async Task Should_fail_binding_reader()
        {
            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-type: application/json\r\n" +
                "\r\n" +
                "{\"value2\":\"fark\"}\r\n" +
                "--some-boundary--", 
                RequestGraph
                    .CreateFor<ModelReaderHandler>(h => h.Post(null))
                    .WithRequestParameter("request")
                    .GetActionDescriptor(),
                new JsonReader(new JsonSerializer()));

            var arguments = new object[1];
            var result = await binder.Bind(new RequestBinderContext(arguments));
            
            result.Status.ShouldEqual(BindingStatus.Failure);
            result.ErrorMessage.ShouldNotBeEmpty();
        }
        
        public class InputStreamEnumerationParameterHandler
        {
            public void Post(IEnumerable<InputStream> files, string value) { }
        }

        [Test]
        public async Task Should_bind_input_stream_enumerable_to_action_parameter(
            [Values(true, false)] bool request)
        {
            var requestGraph = RequestGraph
                .CreateFor<InputStreamEnumerationParameterHandler>(h => h.Post(null, null))
                .AddParameters("value");

            if (request) requestGraph.WithRequestParameter("files");
            else requestGraph.AddParameters("files");

            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-disposition: form-data; name=\"Value\"\r\n" +
                "\r\n" +
                "fark\r\n" +
                "--some-boundary\r\n" +
                "content-type: image/png\r\n" +
                "\r\n" +
                "some image\r\n" +
                "--some-boundary\r\n" +
                "content-type: text/plain\r\n" +
                "\r\n" +
                "some text\r\n" +
                "--some-boundary--", 
                requestGraph.GetActionDescriptor(),
                new JsonReader(new JsonSerializer()));

            var arguments = new object[2];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Success);
            result.ErrorMessage.ShouldBeNull();

            var files = arguments[0] as IEnumerable<InputStream>;

            files.ShouldNotBeNull();

            var file = files.First();

            file.ContentType.ShouldEqual("image/png");
            file.Data.ReadToEnd().ShouldEqual("some image");

            file = files.First();

            file.ContentType.ShouldEqual("text/plain");
            file.Data.ReadToEnd().ShouldEqual("some text");
        }
        
        public class InputStreamEnumerationPropertyHandler
        {
            public class Model
            {
                public IEnumerable<InputStream> Files { get; set; }
            }

            public void Post(Model request) { }
        }

        [Test]
        public async Task Should_bind_input_stream_enumerable_to_model_property()
        {
            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-type: image/png\r\n" +
                "\r\n" +
                "some image\r\n" +
                "--some-boundary\r\n" +
                "content-type: text/plain\r\n" +
                "\r\n" +
                "some text\r\n" +
                "--some-boundary--", 
                RequestGraph
                    .CreateFor<InputStreamEnumerationPropertyHandler>(h => h.Post(null))
                    .WithRequestParameter("request")
                    .GetActionDescriptor(),
                new JsonReader(new JsonSerializer()));

            var arguments = new object[1];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Success);
            result.ErrorMessage.ShouldBeNull();

            var model = arguments[0] as InputStreamEnumerationPropertyHandler.Model;

            model.ShouldNotBeNull();

            var file = model.Files.First();

            file.ContentType.ShouldEqual("image/png");
            file.Data.ReadToEnd().ShouldEqual("some image");

            file =  model.Files.First();

            file.ContentType.ShouldEqual("text/plain");
            file.Data.ReadToEnd().ShouldEqual("some text");
        }
        
        public class StreamEnumerationParameterHandler
        {
            public void Post(IEnumerable<Stream> files, string value) { }
        }

        [Test]
        public async Task Should_bind_stream_enumerable_to_action_parameter(
            [Values(true, false)] bool request)
        {
            var requestGraph = RequestGraph
                .CreateFor<StreamEnumerationParameterHandler>(h => h.Post(null, null))
                .AddParameters("value");

            if (request) requestGraph.WithRequestParameter("files");
            else requestGraph.AddParameters("files");

            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-disposition: form-data; name=\"Value\"\r\n" +
                "\r\n" +
                "fark\r\n" +
                "--some-boundary\r\n" +
                "content-type: image/png\r\n" +
                "\r\n" +
                "some image\r\n" +
                "--some-boundary\r\n" +
                "content-type: text/plain\r\n" +
                "\r\n" +
                "some text\r\n" +
                "--some-boundary--", 
                requestGraph.GetActionDescriptor(),
                new JsonReader(new JsonSerializer()));

            var arguments = new object[2];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Success);
            result.ErrorMessage.ShouldBeNull();

            var files = arguments[0] as IEnumerable<Stream>;

            files.ShouldNotBeNull();

            var file = files.First();
            
            file.ReadToEnd().ShouldEqual("some image");

            file = files.First();
            
            file.ReadToEnd().ShouldEqual("some text");
        }
        
        public class StreamEnumerationPropertyHandler
        {
            public class Model
            {
                public IEnumerable<Stream> Files { get; set; }
            }

            public void Post(Model request) { }
        }

        [Test]
        public async Task Should_bind_stream_enumerable_to_model_property()
        {
            var binder = CreateBinder(
                "--some-boundary\r\n" +
                "content-type: image/png\r\n" +
                "\r\n" +
                "some image\r\n" +
                "--some-boundary\r\n" +
                "content-type: text/plain\r\n" +
                "\r\n" +
                "some text\r\n" +
                "--some-boundary--", 
                RequestGraph
                    .CreateFor<StreamEnumerationPropertyHandler>(h => h.Post(null))
                    .WithRequestParameter("request")
                    .GetActionDescriptor(),
                new JsonReader(new JsonSerializer()));

            var arguments = new object[1];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Success);
            result.ErrorMessage.ShouldBeNull();

            var model = arguments[0] as StreamEnumerationPropertyHandler.Model;

            model.ShouldNotBeNull();

            var file = model.Files.First();
            
            file.ReadToEnd().ShouldEqual("some image");

            file =  model.Files.First();
            
            file.ReadToEnd().ShouldEqual("some text");
        }
        
        public class ModelReaderParameterAndPropertiesHandler
        {
            public class Model
            {
                public string Value1 { get; set; }
                public int Value2 { get; set; }
                public string Value3 { get; set; }
                public int Value4 { get; set; }
            }

            public void Post(Model request, string value3, int value4,
                IEnumerable<InputStream> files) { }
        }

        [Test]
        public async Task Should_bind_matching_reader_and_properties_and_parameters_and_files([Values(
            // Json first
            "--some-boundary\r\n" +
            "content-type: application/json\r\n" +
            "\r\n" +
            "{\"value1\":\"fark\",\"value2\":5}\r\n" +
            "--some-boundary\r\n" +
            "content-disposition: form-data; name=\"value3\"\r\n" +
            "\r\n" +
            "farker\r\n" +
            "--some-boundary\r\n" +
            "content-disposition: form-data; name=\"value4\"\r\n" +
            "\r\n" +
            "6\r\n" +
            "--some-boundary\r\n" +
            "content-type: image/png\r\n" +
            "\r\n" +
            "some image\r\n" +
            "--some-boundary\r\n" +
            "content-type: text/plain\r\n" +
            "\r\n" +
            "some text\r\n" +
            "--some-boundary--",

            // Json last
            "--some-boundary\r\n" +
            "content-disposition: form-data; name=\"value3\"\r\n" +
            "\r\n" +
            "farker\r\n" +
            "--some-boundary\r\n" +
            "content-disposition: form-data; name=\"value4\"\r\n" +
            "\r\n" +
            "6\r\n" +
            "--some-boundary\r\n" +
            "content-type: application/json\r\n" +
            "\r\n" +
            "{\"value1\":\"fark\",\"value2\":5}\r\n" +
            "--some-boundary\r\n" +
            "content-type: image/png\r\n" +
            "\r\n" +
            "some image\r\n" +
            "--some-boundary\r\n" +
            "content-type: text/plain\r\n" +
            "\r\n" +
            "some text\r\n" +
            "--some-boundary--")] string content)
        {
            var binder = CreateBinder(content, 
                RequestGraph
                    .CreateFor<ModelReaderParameterAndPropertiesHandler>(h => h.Post(null, null, 0, null))
                    .WithRequestParameter("request")
                    .AddParameters("value3", "value4", "files")
                    .GetActionDescriptor(),
                new JsonReader(new JsonSerializer()));

            var arguments = new object[4];
            var result = await binder.Bind(new RequestBinderContext(arguments));

            result.Status.ShouldEqual(BindingStatus.Success);
            result.ErrorMessage.ShouldBeNull();

            var model = arguments[0] as ModelReaderParameterAndPropertiesHandler.Model;

            model.ShouldNotBeNull();
            model.Value1.ShouldEqual("fark");
            model.Value2.ShouldEqual(5);
            model.Value3.ShouldEqual("farker");
            model.Value4.ShouldEqual(6);
            
            arguments[1].ShouldEqual("farker");
            arguments[2].ShouldEqual(6);

            var files = arguments[3] as IEnumerable<InputStream>;

            files.ShouldNotBeNull();

            var file = files.First();

            file.ContentType.ShouldEqual("image/png");
            file.Data.ReadToEnd().ShouldEqual("some image");

            file = files.First();

            file.ContentType.ShouldEqual("text/plain");
            file.Data.ReadToEnd().ShouldEqual("some text");
        }

        private MultipartFormBinder CreateBinder(string content, 
            ActionDescriptor actionDescriptor,
            params IRequestReader[] readers)
        {
            var request = new HttpRequestMessage
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = 
                new MediaTypeHeaderValue("multipart/form-data")
                {
                    Parameters =
                    {
                        new NameValueHeaderValue("boundary", "some-boundary")
                    }
                };
            var configuration = new Configuration();
            var parameterBinder = new ParameterBinder<BindResult>(configuration, 
                null, actionDescriptor.Action, actionDescriptor.Route, 
                new SimpleTypeMapper(new ParsedValueMapper()).AsList());
            var argumentBinder = new ArgumentBinder(parameterBinder);
            return new MultipartFormBinder(request, actionDescriptor, 
                readers, argumentBinder, parameterBinder, configuration);
        }

    }
}
