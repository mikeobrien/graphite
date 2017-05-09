using System.Text;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Writers
{
    public class XmlWriter : StringWriterBase
    {
        public XmlWriter() : base(MimeTypes.ApplicationXml, Encoding.UTF8) { }

        protected override string GetResponse(ResponseWriterContext context)
        {
            return context.Response.SerializeXml(context
                .RequestContext.Route.ResponseType.Type);
        }
    }
}