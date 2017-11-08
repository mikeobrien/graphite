using System.Threading.Tasks;
using Graphite.Extensions;

namespace Graphite.Readers
{
    public abstract class StringReaderBase : IRequestReader
    {
        private readonly string[] _mimeTypes;

        protected StringReaderBase(params string[] mimeTypes)
        {
            _mimeTypes = mimeTypes;
        }

        protected abstract ReadResult GetRequest(string data, ReaderContext context);

        public bool AppliesTo(ReaderContext context)
        {
            return context.ContentType.ContentTypeIs(_mimeTypes);
        }

        public async Task<ReadResult> Read(ReaderContext context)
        {
            var data = await context.Data.ReadAsString();
            return GetRequest(data, context);
        }
    }
}