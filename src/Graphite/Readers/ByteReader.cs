using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Extensions;

namespace Graphite.Readers
{
    public class ByteReader : BodyReaderBase<byte[], InputBytes>
    {
        protected override async Task<byte[]> GetData(ReaderContext context)
        {
            return await context.Data.ReadAsByteArray();
        }

        protected override InputBytes GetResult(
            ReaderContext context, object data)
        {
            return context.CreateInputBytes((byte[])data);
        }
    }
}