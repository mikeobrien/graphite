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

        protected abstract object GetRequest(string data, RequestReaderContext context);

        public virtual bool AppliesTo(RequestReaderContext context)
        {
            return context.RequestContext.Route.HasRequest && context
                .RequestContext.RequestMessage.ContentTypeIs(_mimeTypes);
        }

        public async Task<object> Read(RequestReaderContext context)
        {
            var data = await context.RequestContext.RequestMessage
                .Content.ReadAsStringAsync();
            return GetRequest(data, context);
        }
    }
}