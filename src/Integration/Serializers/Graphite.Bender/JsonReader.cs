using System.Net.Http;
using Bender;
using Bender.Configuration;
using Graphite.Binding;
using Graphite.Http;
using Graphite.Readers;
using Graphite.Routing;

namespace Graphite.Bender
{
    public class JsonReader : StringReaderBase
    {
        private readonly Options _options;
        private readonly RouteDescriptor _routeDescriptor;

        public JsonReader(Options options, RouteDescriptor routeDescriptor,
            HttpRequestMessage requestMessage) : 
            base(requestMessage, MimeTypes.ApplicationJson)
        {
            _options = options;
            _routeDescriptor = routeDescriptor;
        }

        protected override object GetRequest(string data)
        {
            try
            {
                return data.DeserializeJson(_routeDescriptor
                    .RequestParameter.ParameterType.Type, _options);
            }
            catch (FriendlyBenderException exception)
            {
                throw new BadRequestException(exception.FriendlyMessage, 
                    exception.InnerException);
            }
        }
    }
}