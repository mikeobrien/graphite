using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Graphite.Extensions
{
    public static class IOExtensions
    {
        public static Task WriteAsync(this Stream stream, byte[] buffer)
        {
            return stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static Task WriteAsync(this Stream stream, string data, Encoding encoding)
        {
            return stream.WriteAsync(encoding.GetBytes(data));
        }

        public static string ReadToEnd(this Stream stream, Encoding encoding)
        {
            if (stream == null) return null;
            using (var reader = new System.IO.StreamReader(stream, encoding))
            {
                return reader.ReadToEnd();
            }
        }

        public static StreamWriter CreateWriter(this Stream stream, Encoding encoding, int? bufferSize)
        {
            return bufferSize.HasValue
                ? new StreamWriter(stream, encoding, bufferSize.Value)
                : new StreamWriter(stream, encoding);
        }
    }
}
