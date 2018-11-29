using System.IO;
using System.Net.Http;
using System.Xml;
using System.Xml.Serialization;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Writers
{
    public class XmlWriter : SerializerWriterBase
    {
        private readonly Configuration _configuration;
        private readonly XmlWriterSettings _xmlWriterSettings;

        public XmlWriter(
            HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage,
            Configuration configuration,
            XmlWriterSettings xmlWriterSettings) :
            base(requestMessage, responseMessage,
                configuration, MimeTypes.ApplicationXml)
        {
            _configuration = configuration;
            _xmlWriterSettings = xmlWriterSettings;
        }

        protected override void WriteToStream(ResponseWriterContext context, Stream output)
        {
            using (var streamWriter = output.CreateWriter(_configuration.DefaultEncoding,
                _configuration.SerializerBufferSize, true))
            using (var writer = System.Xml.XmlWriter.Create(streamWriter, _xmlWriterSettings))
            {
                new XmlSerializer(context.Response.GetType())
                    .Serialize(writer, context.Response);
            }
        }
    }
}