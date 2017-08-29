using System.Net.Http;
using Graphite.Http;
using Graphite.Routing;
using Newtonsoft.Json;

namespace Graphite.Readers
{
    public class JsonReader : StringReaderBase
    {
        private readonly JsonSerializer _serializer;
        private readonly RouteDescriptor _routeDescriptor;

        public JsonReader(JsonSerializer serializer,
            RouteDescriptor routeDescriptor, HttpRequestMessage requestMessage) : 
            base(requestMessage, MimeTypes.ApplicationJson)
        {
            _serializer = serializer;
            _routeDescriptor = routeDescriptor;
        }

        protected override ReadResult GetRequest(string data)
        {
            try
            {
                var jsonReader = new JsonTextReader(new System.IO.StringReader(data));
                return ReadResult.Success(_serializer.Deserialize(jsonReader, _routeDescriptor
                    .RequestParameter.ParameterType.Type));
            }
            catch (JsonReaderException exception)
            {
                return ReadResult.Failure(exception.Message);
            }
            catch (JsonSerializationException exception)
            {
                return ReadResult.Failure(exception.Message);
            }
        }
    }
}