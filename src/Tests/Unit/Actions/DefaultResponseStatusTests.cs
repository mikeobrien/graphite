using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Graphite;
using Graphite.Actions;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Actions
{
    [TestFixture]
    public class DefaultResponseStatusTests
    {
        public class Handler
        {
            public void Post() { }

            [NoReaderStatus(HttpStatusCode.Created)]
            [BindingFailureStatus(HttpStatusCode.Gone)]
            [HasResponseStatus(HttpStatusCode.Forbidden)]
            [NoResponseStatus(HttpStatusCode.Created)]
            [NoWriterStatus(HttpStatusCode.Moved)]
            public void OverrideCodePost() { }

            [NoReaderStatus(HttpStatusCode.Created, "no reader")]
            [HasResponseStatus(HttpStatusCode.Forbidden, "has response")]
            [NoResponseStatus(HttpStatusCode.Created, "no response")]
            [NoWriterStatus(HttpStatusCode.Moved, "no writer")]
            public void OverrideCodeAndTextPost() { }
        }
        
        private Configuration _configuration;
        private HttpResponseMessage _response;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _response = new HttpResponseMessage();
        }

        [Test]
        public void Should_set_default_no_reader_status()
        {
            CreateStatus(ActionMethod.From<Handler>(x => x.Post()))
                .SetStatus(CreateContext(ResponseState.NoReader));

            _response.StatusCode.ShouldEqual(_configuration.DefaultNoReaderStatusCode);
            _response.ReasonPhrase.ShouldEqual(_configuration.DefaultNoReaderReasonPhrase);
        }

        [Test]
        public void Should_override_no_reader_code()
        {
            CreateStatus(ActionMethod.From<Handler>(x => x.OverrideCodePost()))
                .SetStatus(CreateContext(ResponseState.NoReader));

            _response.StatusCode.ShouldEqual(HttpStatusCode.Created);
            _response.ReasonPhrase.ShouldEqual(_configuration.DefaultNoReaderReasonPhrase);
        }

        [Test]
        public void Should_override_no_reader_code_and_text()
        {
            CreateStatus(ActionMethod.From<Handler>(x => x.OverrideCodeAndTextPost()))
                .SetStatus(CreateContext(ResponseState.NoReader));

            _response.StatusCode.ShouldEqual(HttpStatusCode.Created);
            _response.ReasonPhrase.ShouldEqual("no reader");
        }

        [Test]
        public void Should_set_default_binding_failure_status()
        {
            CreateStatus(ActionMethod.From<Handler>(x => x.Post()))
                .SetStatus(CreateContext(ResponseState.BindingFailure));

            _response.StatusCode.ShouldEqual(_configuration.DefaultBindingFailureStatusCode);
            _response.ReasonPhrase.ShouldEqual("Bad Request");
        }

        [Test]
        public void Should_override_binding_failure_code()
        {
            CreateStatus(ActionMethod.From<Handler>(x => x.OverrideCodePost()))
                .SetStatus(CreateContext(ResponseState.BindingFailure, "fark"));

            _response.StatusCode.ShouldEqual(HttpStatusCode.Gone);
            _response.ReasonPhrase.ShouldEqual("fark");
        }

        [Test]
        public void Should_set_default_has_response_status(
            [Values(null, "fark")] string reasonPhrase)
        {
            _configuration.DefaultHasResponseReasonPhrase = reasonPhrase;
            CreateStatus(ActionMethod.From<Handler>(x => x.Post()))
                .SetStatus(CreateContext(ResponseState.HasResponse));

            _response.StatusCode.ShouldEqual(_configuration.DefaultHasResponseStatusCode);
            _response.ReasonPhrase.ShouldEqual(reasonPhrase ?? "OK");
        }

        [Test]
        public void Should_override_has_response_code(
            [Values(null, "fark")] string reasonPhrase)
        {
            _configuration.DefaultHasResponseReasonPhrase = reasonPhrase;
            CreateStatus(ActionMethod.From<Handler>(x => x.OverrideCodePost()))
                .SetStatus(CreateContext(ResponseState.HasResponse));

            _response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
            _response.ReasonPhrase.ShouldEqual(reasonPhrase ?? "Forbidden");
        }

        [Test]
        public void Should_override_has_response_code_and_text()
        {
            CreateStatus(ActionMethod.From<Handler>(x => x.OverrideCodeAndTextPost()))
                .SetStatus(CreateContext(ResponseState.HasResponse));

            _response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
            _response.ReasonPhrase.ShouldEqual("has response");
        }

        [Test]
        public void Should_set_default_no_response_status(
            [Values(null, "fark")] string reasonPhrase)
        {
            _configuration.DefaultNoResponseReasonPhrase = reasonPhrase;
            CreateStatus(ActionMethod.From<Handler>(x => x.Post()))
                .SetStatus(CreateContext(ResponseState.NoResponse));

            _response.StatusCode.ShouldEqual(_configuration.DefaultNoResponseStatusCode);
            _response.ReasonPhrase.ShouldEqual(reasonPhrase ?? "No Content");
        }

        [Test]
        public void Should_override_no_response_code(
            [Values(null, "fark")] string reasonPhrase)
        {
            _configuration.DefaultNoResponseReasonPhrase = reasonPhrase;
            CreateStatus(ActionMethod.From<Handler>(x => x.OverrideCodePost()))
                .SetStatus(CreateContext(ResponseState.NoResponse));

            _response.StatusCode.ShouldEqual(HttpStatusCode.Created);
            _response.ReasonPhrase.ShouldEqual(reasonPhrase ?? "Created");
        }

        [Test]
        public void Should_override_no_response_code_and_text()
        {
            CreateStatus(ActionMethod.From<Handler>(x => x.OverrideCodeAndTextPost()))
                .SetStatus(CreateContext(ResponseState.NoResponse));

            _response.StatusCode.ShouldEqual(HttpStatusCode.Created);
            _response.ReasonPhrase.ShouldEqual("no response");
        }

        [Test]
        public void Should_set_default_no_writer_status()
        {
            CreateStatus(ActionMethod.From<Handler>(x => x.Post()))
                .SetStatus(CreateContext(ResponseState.NoWriter));

            _response.StatusCode.ShouldEqual(_configuration.DefaultNoWriterStatusCode);
            _response.ReasonPhrase.ShouldEqual(_configuration.DefaultNoWriterReasonPhrase);
        }

        [Test]
        public void Should_override_no_writer_code()
        {
            CreateStatus(ActionMethod.From<Handler>(x => x.OverrideCodePost()))
                .SetStatus(CreateContext(ResponseState.NoWriter));

            _response.StatusCode.ShouldEqual(HttpStatusCode.Moved);
            _response.ReasonPhrase.ShouldEqual(_configuration.DefaultNoWriterReasonPhrase);
        }

        [Test]
        public void Should_override_no_writer_code_and_text()
        {
            CreateStatus(ActionMethod.From<Handler>(x => x.OverrideCodeAndTextPost()))
                .SetStatus(CreateContext(ResponseState.NoWriter));

            _response.StatusCode.ShouldEqual(HttpStatusCode.Moved);
            _response.ReasonPhrase.ShouldEqual("no writer");
        }

        private DefaultResponseStatus CreateStatus(ActionMethod actionMethod)
        {
            return new DefaultResponseStatus(_configuration, actionMethod);
        }

        private ResponseStatusContext CreateContext(ResponseState state, string errorMessage = null)
        {
            return new ResponseStatusContext(_response, state, errorMessage);
        }
    }
}
