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

        public static string ReadToEnd(this Stream stream, Encoding encoding = null)
        {
            if (stream == null) return null;
            using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static StreamWriter CreateWriter(this Stream stream, 
            Encoding encoding, int? bufferSize)
        {
            return bufferSize.HasValue
                ? new StreamWriter(stream, encoding, bufferSize.Value)
                : new StreamWriter(stream, encoding);
        }

        public static async Task<byte[]> ReadAsByteArray(this Task<Stream> stream)
        {
            using (var buffer = new MemoryStream())
            {
                (await stream).CopyTo(buffer);
                return buffer.ToArray();
            }
        }

        public static async Task<string> ReadAsString(
            this Task<Stream> stream, Encoding encoding = null)
        {
            return (await stream).ReadToEnd(encoding ?? Encoding.UTF8);
        }
    }
}
