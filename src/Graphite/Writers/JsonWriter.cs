using System;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Newtonsoft.Json;

namespace Graphite.Writers
{
    public class JsonWriter : WeightedContentWriterBase
    {
        private readonly JsonSerializer _serializer;
        private readonly Configuration _configuration;

        public JsonWriter(JsonSerializer serializer,
            HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage,
            Configuration configuration) :
            base(requestMessage, responseMessage,
                configuration, MimeTypes.ApplicationJson)
        {
            _serializer = serializer;
            _configuration = configuration;
        }
        
        protected override HttpContent GetContent(ResponseWriterContext context)
        {
            return new AsyncContent(output =>
            {
                var streamWriter = output.CreateWriter(_configuration.DefaultEncoding,
                    _configuration.SerializerBufferSize);
                var jsonWriter = new JsonTextWriter(streamWriter);
                _serializer.Serialize(jsonWriter, context.Response);
                jsonWriter.Flush();
                if (_configuration.DisposeSerializedObjects)
                    context.Response.As<IDisposable>()?.Dispose();
                return Task.CompletedTask;
            });
        }
    }
}