using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using Graphite.Collections;
using Graphite.Exceptions;
using Graphite.Http;

namespace Graphite.Binding
{
    public class MultipartContent : IEnumerable<InputStream>
    {
        private readonly MultipartReader _reader;
        private MultipartPartContent _peeked;
        private IEnumerator<InputStream> _enumerator;
        private MultipartPartContent _currentPart;

        public MultipartContent(Stream stream, HttpContentHeaders headers, Configuration configuration)
        {
            _reader = new MultipartReader(stream, headers, configuration.DefaultBufferSize);
        }

        public MultipartPartContent Pop()
        {
            return PopPeeked() ??
                (_reader.EndOfStream
                    ? null
                    : GetPart());
        }

        private MultipartPartContent PopPeeked()
        {
            if (_peeked == null) return null;
            var peeked = _peeked;
            _peeked = null;
            return peeked;
        }

        public MultipartPartContent Peek()
        {
            if (_peeked != null) return _peeked;
            return _reader.EndOfStream 
                ? null 
                : _peeked = GetPart();
        }
        
        public IEnumerable<Stream> GetStreams()
        {
            return new EnumerableMapper<InputStream, Stream>(
                GetEnumerator(), x => x.Data);
        }
        
        private IEnumerable<InputStream> EnumerateInputStreams()
        {
            while (!_reader.EndOfStream && !_reader.IsInEpilogue)
            {
                var part = PopPeeked() ?? GetPart();

                if (part?.Error ?? false) throw new BadRequestException(part.ErrorMessage);

                if (!_reader.IsInEpilogue && !_reader.EndOfStream)
                    yield return part.CreateInputStream();
            }
        }

        private MultipartPartContent GetPart()
        {
            if (_reader.IsInPreamble || !(_currentPart?.ReadComplete ?? true))
            {
                var result = _reader.ReadToNextPart();
                if (result.Error)
                    return new MultipartPartContent(result.ErrorMessage);
            }

            if (_reader.EndOfStream || _reader.IsInEpilogue)
                return null;

            var headers = _reader.IsInHeaders
                ? _reader.ReadString()
                : null;

            _currentPart = new MultipartPartContent(_reader, headers?.Data,
                headers?.Error ?? false, headers?.ErrorMessage);

            return _currentPart;
        }

        public IEnumerator<InputStream> GetEnumerator()
        {
            return _enumerator ?? (_enumerator =
                EnumerateInputStreams().GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
