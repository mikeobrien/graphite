using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Graphite.Extensions
{
    public static class SerializerExtensions
    {
        public static string SerializeXml(this object source, Type type)
        {
            using (var stream = new MemoryStream())
            {
                new XmlSerializer(type).Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public static object DeserializeXml(this string source, Type type, Encoding encoding)
        {
            using (var stream = new MemoryStream(encoding.GetBytes(source)))
            {
                return new XmlSerializer(type).Deserialize(stream);
            }
        }

        public static string SerializeJson(this object source, Type type)
        {
            return new JavaScriptSerializer().Serialize(source);
        }

        public static object DeserializeJson(this string source, Type type)
        {
            return new JavaScriptSerializer().Deserialize(source, type);
        }
    }
}
