using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Routing;

namespace Graphite.Readers
{
    public abstract class InputBody<T>
    {
        public string Filename { get; set; }
        public string MimeType { get; set; }
        public long? Length { get; set; }
        public T Data { get; set; }
    }

    public abstract class BodyReaderBase<T, TWrapper> : IRequestReader 
        where TWrapper : InputBody<T>, new()
    {
        private readonly RouteDescriptor _routeDescriptor;
        private readonly HttpRequestMessage _requestMessage;

        protected BodyReaderBase(RouteDescriptor routeDescriptor, HttpRequestMessage requestMessage)
        {
            _routeDescriptor = routeDescriptor;
            _requestMessage = requestMessage;
        }

        public bool Applies()
        {
            var requestType = _routeDescriptor.RequestParameter?.ParameterType.Type;
            return requestType == typeof(T) || requestType == typeof(TWrapper);
        }

        protected abstract Task<T> GetData(HttpContent content);

        public async Task<ReadResult> Read()
        {
            var requestType = _routeDescriptor.RequestParameter?.ParameterType.Type;
            var data = await GetData(_requestMessage.Content);
            if (requestType == typeof(T)) return ReadResult.Success(data);
            var headers = _requestMessage.Content.Headers;
            return ReadResult.Success(new TWrapper
            {
                Filename = headers.ContentDisposition?.FileName.Trim('"'),
                MimeType = headers.ContentType?.MediaType,
                Length = headers.ContentLength,
                Data = data
            });
        }
    }
}