using System.Net.Http;
using Graphite.Http;
using Graphite.Routing;
using Newtonsoft.Json;

namespace Graphite.Readers
{
    public class JsonReader : StringReaderBase
    {
        private readonly JsonSerializerSettings _settings;
        private readonly RouteDescriptor _routeDescriptor;

        public JsonReader(JsonSerializerSettings settings,
            RouteDescriptor routeDescriptor, HttpRequestMessage requestMessage) : 
            base(requestMessage, MimeTypes.ApplicationJson)
        {
            _settings = settings;
            _routeDescriptor = routeDescriptor;
        }

        protected override object GetRequest(string data)
        {
            return JsonConvert.DeserializeObject(data, _routeDescriptor
                .RequestParameter.ParameterType.Type, _settings);
        }
    }
}