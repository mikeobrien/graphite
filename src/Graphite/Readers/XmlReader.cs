using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Graphite.Http;

namespace Graphite.Readers
{
    public class XmlReader : StringReaderBase
    {
        private readonly Configuration _configuration;
        private readonly XmlReaderSettings _xmlReaderSettings;

        public XmlReader(Configuration configuration,
            XmlReaderSettings xmlReaderSettings) : 
            base(MimeTypes.ApplicationXml)
        {
            _configuration = configuration;
            _xmlReaderSettings = xmlReaderSettings;
        }

        protected override ReadResult GetRequest(string data, ReaderContext context)
        {
            try
            {
                using (var stream = new MemoryStream(_configuration
                    .DefaultEncoding.GetBytes(data)))
                {
                    var reader = System.Xml.XmlReader.Create(stream, _xmlReaderSettings);
                    return ReadResult.Success(new XmlSerializer(
                        context.ReadType.Type).Deserialize(reader));
                }
            }
            catch (InvalidOperationException exception)
            {
                return ReadResult.Failure(exception.Message);
            }
        }
    }
}