using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Graphite.Http;

namespace Graphite.Writers
{
    public class XmlWriter : PushWriterBase
    {
        private readonly XmlWriterSettings _xmlWriterSettings;

        public XmlWriter(
            HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage,
            Configuration configuration,
            XmlWriterSettings xmlWriterSettings) :
            base(requestMessage, responseMessage,
                configuration, MimeTypes.ApplicationXml)
        {
            _xmlWriterSettings = xmlWriterSettings;
        }

        protected override Task WriteResponse(ResponseWriterContext context,
            Stream stream, TransportContext transportContext)
        {
            var writer = System.Xml.XmlWriter.Create(stream, _xmlWriterSettings);
            new XmlSerializer(context.Response.GetType())
                .Serialize(writer, context.Response);
            return Task.CompletedTask;
        }
    }
}