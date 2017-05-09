using Bender;
using Bender.Configuration;
using Graphite.Http;
using Graphite.Readers;

namespace Graphite.Bender
{
    public class XmlReader : StringReaderBase
    {
        private readonly Options _options;

        public XmlReader(Options options) : base(MimeTypes.ApplicationXml)
        {
            _options = options;
        }

        protected override object GetRequest(string data, RequestReaderContext context)
        {
            return data.DeserializeXml(context.RequestContext.Route
                .RequestParameter.ParameterType.Type, _options);
        }
    }
}