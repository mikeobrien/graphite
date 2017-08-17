using System;
using System.Xml;
using Newtonsoft.Json;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Allows you to configure serialization.
        /// </summary>
        public ConfigurationDsl ConfigureSerialization(Action<SerializationDsl> configure)
        {
            configure?.Invoke(new SerializationDsl(_configuration));
            return this;
        }
    }

    public class SerializationDsl
    {
        private readonly Configuration _configuration;

        public SerializationDsl(Configuration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Allows you to configure Json.NET.
        /// </summary>
        public SerializationDsl Json(Action<JsonSerializerSettings> configure)
        {
            configure?.Invoke(_configuration.JsonSerializerSettings);
            return this;
        }

        /// <summary>
        /// Allows you to configure the XML serializer.
        /// </summary>
        public SerializationDsl Xml(Action<XmlSerializationDsl> configure)
        {
            configure?.Invoke(new XmlSerializationDsl(_configuration));
            return this;
        }

        /// <summary>
        /// Specifies the serializer buffer size in bytes.
        /// </summary>
        public SerializationDsl WithBufferSizeOf(int length)
        {
            _configuration.SerializerBufferSize = length;
            return this;
        }

        /// <summary>
        /// Indicates that objects should be disposed after they
        /// have been serialized, if they implement IDisposable.
        /// </summary>
        public SerializationDsl DisposeSerializedObjects()
        {
            _configuration.DisposeSerializedObjects = true;
            return this;
        }
    }

    public class XmlSerializationDsl
    {
        private readonly Configuration _configuration;

        public XmlSerializationDsl(Configuration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Allows you to configure the XML writer.
        /// </summary>
        public XmlSerializationDsl Writer(Action<XmlWriterSettings> configure)
        {
            configure?.Invoke(_configuration.XmlWriterSettings);
            return this;
        }

        /// <summary>
        /// Allows you to configure the XML reader.
        /// </summary>
        public XmlSerializationDsl Reader(Action<XmlReaderSettings> configure)
        {
            configure?.Invoke(_configuration.XmlReaderSettings);
            return this;
        }
    }
}