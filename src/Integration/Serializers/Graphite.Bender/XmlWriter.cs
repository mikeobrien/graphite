using System.Text;
using Bender;
using Bender.Configuration;
using Graphite.Http;
using Graphite.Writers;

namespace Graphite.Bender
{
    public class XmlWriter : StringWriterBase
    {
        private readonly Options _options;

        public XmlWriter(Options options) : base(MimeTypes.ApplicationXml, Encoding.UTF8)
        {
            _options = options;
        }

        protected override string GetResponse(ResponseWriterContext context)
        {
            return context.Response.SerializeXml(context
                .RequestContext.Route.ResponseType.Type, _options);
        }
    }
}