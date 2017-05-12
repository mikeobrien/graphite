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
    }
}
