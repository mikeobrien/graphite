using System.Text;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Readers
{
    public class XmlReader : StringReaderBase
    {
        public XmlReader() : base(MimeTypes.ApplicationXml) { }

        protected override object GetRequest(string data, RequestReaderContext context)
        {
            return data.DeserializeXml(context.RequestContext.Route
                .RequestParameter.ParameterType.Type, Encoding.UTF8);
        }
    }
}