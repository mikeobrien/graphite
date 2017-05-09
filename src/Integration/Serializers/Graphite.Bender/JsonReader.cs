using Bender;
using Bender.Configuration;
using Graphite.Binding;
using Graphite.Http;
using Graphite.Readers;

namespace Graphite.Bender
{
    public class JsonReader : StringReaderBase
    {
        private readonly Options _options;

        public JsonReader(Options options) : base(MimeTypes.ApplicationJson)
        {
            _options = options;
        }

        protected override object GetRequest(string data, 
            RequestReaderContext context)
        {
            try
            {
                return data.DeserializeJson(context.RequestContext.Route
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