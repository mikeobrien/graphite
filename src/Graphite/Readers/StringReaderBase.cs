using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensions;

namespace Graphite.Readers
{
    public abstract class StringReaderBase : IRequestReader
    {
        private readonly HttpRequestMessage _requestMessage;
        private readonly string[] _mimeTypes;

        protected StringReaderBase(HttpRequestMessage requestMessage, 
            params string[] mimeTypes)
        {
            _requestMessage = requestMessage;
            _mimeTypes = mimeTypes;
        }

        protected abstract object GetRequest(string data);

        public virtual bool Applies()
        {
            return _requestMessage.ContentTypeIs(_mimeTypes);
        }

        public async Task<object> Read()
        {
            var data = await _requestMessage.Content.ReadAsStringAsync();
            return GetRequest(data);
        }
    }
}