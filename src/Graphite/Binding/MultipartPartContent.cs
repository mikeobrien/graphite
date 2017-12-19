using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Binding
{
    public class MultipartPartContent : HttpContent
    {
        private readonly MultipartPartStream _stream;

        public MultipartPartContent(string errorMessage)
        {
            Error = true;
            ErrorMessage = errorMessage;
        }

        public MultipartPartContent(MultipartReader reader, 
            string headers, bool error, string errorMessage)
        {
            Error = error;
            ErrorMessage = errorMessage;

            if (error) return;

            _stream = new MultipartPartStream(reader);
            ParseHeaders(headers);
        }

        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Filename { get; private set; }
        public bool Error { get; }
        public string ErrorMessage { get; }
        public bool ReadComplete => _stream.EndOfPart;

        private void ParseHeaders(string headers)
        {
            if (headers.IsNullOrEmpty()) return;
            
            foreach (var parsedHeader in headers.ParseHeaders())
            {
                if (parsedHeader.Key.EqualsUncase(RequestHeaders.ContentDisposition))
                {
                    if (ContentDispositionHeaderValue.TryParse(
                        parsedHeader.Value, out var contentDisposition))
                    {
                        Headers.ContentDisposition = contentDisposition;
                        contentDisposition.Name = contentDisposition.Name.Unquote();
                        contentDisposition.FileName = contentDisposition.FileName.Unquote();
                        Name = contentDisposition.Name.Unquote();
                        Type = contentDisposition.DispositionType;
                        Filename = contentDisposition.FileName.Unquote();
                    }
                }
                else if (parsedHeader.Key.EqualsUncase(RequestHeaders.ContentType))
                {
                    if (MediaTypeHeaderValue.TryParse(parsedHeader.Value, out var contentType))
                        Headers.ContentType = contentType;
                }
                else if (parsedHeader.Key.EqualsUncase(RequestHeaders.ContentEncoding))
                {
                    var tokens = parsedHeader.Value.ParseTokens();
                    if (tokens?.Any() ?? false)
                        Headers.ContentEncoding.AddRange(tokens);
                }
                else if (parsedHeader.Key.EqualsUncase(RequestHeaders.ContentLength))
                {
                    if (parsedHeader.Value.TryParseInt64(out var length))
                        Headers.ContentLength = length;
                }
                else if (parsedHeader.Key.EqualsUncase(RequestHeaders.ContentLanguage))
                {
                    var tokens = parsedHeader.Value.ParseTokens();
                    if (tokens?.Any() ?? false)
                        Headers.ContentLanguage.AddRange(tokens);
                }
                else Headers.TryAddWithoutValidation(parsedHeader.Key, parsedHeader.Value);
            }
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return _stream.ToTaskResult<Stream>();
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _stream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}