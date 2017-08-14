using System;
using System.IO;
using System.Net.Http;
using System.Xml;
using System.Xml.Serialization;
using Graphite.Binding;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Readers
{
    public class XmlReader : StringReaderBase
    {
        private readonly RouteDescriptor _routeDescriptor;
        private readonly Configuration _configuration;
        private readonly XmlReaderSettings _xmlReaderSettings;

        public XmlReader(RouteDescriptor routeDescriptor, 
            HttpRequestMessage requestMessage,
            Configuration configuration,
            XmlReaderSettings xmlReaderSettings) : 
            base(requestMessage, MimeTypes.ApplicationXml)
        {
            _routeDescriptor = routeDescriptor;
            _configuration = configuration;
            _xmlReaderSettings = xmlReaderSettings;
        }

        protected override object GetRequest(string data)
        {
            try
            {
                using (var stream = new MemoryStream(_configuration.DefaultEncoding.GetBytes(data)))
                {
                    var reader = System.Xml.XmlReader.Create(stream, _xmlReaderSettings);
                    return new XmlSerializer(_routeDescriptor.RequestParameter
                        .ParameterType.Type).Deserialize(reader);
                }
            }
            catch (InvalidOperationException exception)
            {
                throw new BadRequestException(exception.Message, exception);
            }
        }
    }
}