using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Extensions;

namespace Graphite.Readers
{
    public class StringReader : BodyReaderBase<string, InputString>
    {
        protected override async Task<string> GetData(ReaderContext context)
        {
            return await context.Data.ReadAsString();
        }

        protected override InputString GetResult(
            ReaderContext context, object data)
        {
            return context.CreateInputString((string)data);
        }
    }
}