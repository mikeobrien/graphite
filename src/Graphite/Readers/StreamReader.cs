using System.IO;
using System.Threading.Tasks;
using Graphite.Binding;

namespace Graphite.Readers
{
    public class StreamReader : BodyReaderBase<Stream, InputStream>
    {
        protected override Task<Stream> GetData(ReaderContext context)
        {
            return context.Data;
        }

        protected override InputStream GetResult(
            ReaderContext context, object data)
        {
            return context.CreateInputStream((Stream)data);
        }
    }
}