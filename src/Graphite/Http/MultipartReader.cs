using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using Graphite.Extensions;

namespace Graphite.Http
{
    public enum MultipartSection
    {
        Preamble,
        Headers,
        Body,
        Epilogue
    }

    public class MultipartReader
    {
        // Implementation of RFC 2046 5.1.1 (except for nested multitype)
        
        public const string ErrorBoundaryNotPreceededByCRLF = "Boundary not preceeded by CRLF.";
        public const string ErrorHeadersNotFollowedByEmptyLine = "Headers not followed by empty line.";
        public const string ErrorUnexpectedEndOfStream = "Unexpected end of stream.";
        public const string ErrorInvalidCharactersFollowingBoundary = "Invalid characters following boundary.";
        public const string ErrorBoundaryFoundAfterClosingBoundary = "Boundary found after closing boundary.";

        private static readonly byte[] LinearWhitespace = " \r\n\t".ToBytes();
        private static readonly byte[] CRLF = "\r\n".ToBytes();
        private static readonly byte[] BodyDelimiter = "\r\n\r\n".ToBytes();
        private static readonly byte[] EpiloguePostfix = "--".ToBytes();

        private readonly DelimitedBuffer _buffer;
        private readonly byte[] _boundary;
        private readonly byte[] _boundaryLine;
        private MultipartSection _currentSection = MultipartSection.Preamble;

        public MultipartReader(Stream stream, HttpContentHeaders headers,
            int bufferSize = DelimitedBuffer.DefaultBufferSize)
        {
            _buffer = new DelimitedBuffer(stream, bufferSize);

            var boundary = headers.GetContentBoundry();
            if (boundary.IsNullOrEmpty())
                throw new ArgumentException("No boundry specified in the content-type header.");

            _boundary = $"--{boundary}".ToBytes();
            _boundaryLine = $"\r\n--{boundary}".ToBytes();
        }

        public bool EndOfPart { get; private set; }
        public bool EndOfStream { get; private set; }

        public bool IsInPreamble => _currentSection == MultipartSection.Preamble;
        public bool IsInHeaders => _currentSection == MultipartSection.Headers;
        public bool IsInBody => _currentSection == MultipartSection.Body;
        public bool IsInEpilogue => _currentSection == MultipartSection.Epilogue;

        public ReadResult ReadToNextPart()
        {
            while (true)
            {
                var result = Read(null, 0, DelimitedBuffer.DefaultBufferSize);
                if (result.EndOfPart || result.Error) return result;
            }
        }

        public class ReadStringResult : ReadResult
        {
            public ReadStringResult(ReadResult result, string data)
                : base(result)
            {
                Data = data;
            }

            public string Data { get; }
        }

        public ReadStringResult ReadString(Encoding encoding = null)
        {
            var buffer = new byte[DelimitedBuffer.DefaultBufferSize];
            var data = "";
            while (true)
            {
                var result = Read(buffer, 0, DelimitedBuffer.DefaultBufferSize);
                if (result.Read > 0)
                    data += (encoding ?? Encoding.UTF8)
                        .GetString(buffer, 0, result.Read);
                if (result.EndOfPart || result.Error) return new 
                    ReadStringResult(result, data);
            }
        }

        public class ReadResult
        {
            public ReadResult(ReadResult result, 
                string errorMessage = null)
            {
                Read = result.Read;
                Section = result.Section;
                ErrorMessage = errorMessage ?? result.ErrorMessage;
                Error = ErrorMessage.IsNotNullOrEmpty();
                EndOfStream = result.EndOfStream;
                EndOfPart = result.EndOfPart;
            }

            public ReadResult(MultipartSection? section, 
                DelimitedBuffer.ReadResult result, 
                string errorMessage = null)
            {
                Read = result.Read;
                Section = section;
                ErrorMessage = errorMessage;
                Error = ErrorMessage.IsNotNullOrEmpty();
                EndOfStream = result.EndOfStream;
                EndOfPart = result.EndOfSection;
            }

            public ReadResult(string errorMessage)
            {
                Error = errorMessage.IsNotNullOrEmpty();
                ErrorMessage = errorMessage;
            }

            public ReadResult(
                DelimitedBuffer.ReadResult result,
                string errorMessage)
                : this(null, result, errorMessage)  { }

            public int Read { get; }
            public MultipartSection? Section { get; }
            public bool EndOfPart { get; }
            public bool EndOfStream { get; }
            public bool Error { get; }
            public string ErrorMessage { get; }
        }

        public ReadResult Read(byte[] buffer, int offset, int count)
        {
            if (_currentSection == MultipartSection.Preamble)
                return ReadPreamble(buffer, offset, count);

            if (_currentSection == MultipartSection.Headers)
                return ReadHeaders(buffer, offset, count);

            if (_currentSection == MultipartSection.Body)
                return ReadBody(buffer, offset, count);

            return ReadEpilogue(buffer, offset, count);
        }

        private ReadResult ReadPreamble(byte[] buffer, int offset, int count)
        {
            var preambleResult = _buffer.BeginingOfStream && _buffer.StartsWith(_boundary)
                ? _buffer.ReadTo(buffer, offset, count, _boundary)
                : _buffer.ReadTo(buffer, offset, count, _boundaryLine, _boundary);

            if (preambleResult.Invalid)
                return new ReadResult(preambleResult, ErrorBoundaryNotPreceededByCRLF);

            if (preambleResult.EndOfStream)
                return new ReadResult(MultipartSection.Preamble, preambleResult, 
                    ErrorUnexpectedEndOfStream);

            if (!preambleResult.EndOfSection)
                return new ReadResult(MultipartSection.Preamble, preambleResult);

            var boundaryResult = ReadBoundary();
                    
            if (boundaryResult.Error)
                return new ReadResult(preambleResult, boundaryResult.ErrorMessage);

            return new ReadResult(MultipartSection.Preamble, preambleResult);
        }

        private ReadResult ReadHeaders(byte[] buffer, int offset, int count)
        {
            var result = _buffer.ReadTo(buffer, offset, 
                count, BodyDelimiter, _boundaryLine, _boundary);

            if (result.Invalid)
                return new ReadResult(result, ErrorHeadersNotFollowedByEmptyLine);

            if (result.EndOfStream)
                return new ReadResult(result, ErrorUnexpectedEndOfStream);

            if (result.EndOfSection) _currentSection = MultipartSection.Body;

            return new ReadResult(MultipartSection.Headers, result);
        }

        private ReadResult ReadBody(byte[] buffer, int offset, int count)
        {
            var bodyResult = _buffer.ReadTo(buffer, offset, count, _boundaryLine, _boundary);

            if (bodyResult.Invalid)
                return new ReadResult(bodyResult, ErrorBoundaryNotPreceededByCRLF);

            if (bodyResult.EndOfStream)
                return new ReadResult(bodyResult, ErrorUnexpectedEndOfStream);

            if (!bodyResult.EndOfSection)
                return new ReadResult(MultipartSection.Body, bodyResult);

            var boundaryResult = ReadBoundary();

            if (boundaryResult.Error)
                return new ReadResult(MultipartSection.Body, bodyResult,
                    boundaryResult.ErrorMessage);

            return new ReadResult(MultipartSection.Body, bodyResult);
        }

        private ReadResult ReadBoundary()
        {
            var closingBoundary = _buffer.StartsWith(EpiloguePostfix);

            if (closingBoundary && _currentSection == MultipartSection.Preamble)
                return new ReadResult(ErrorUnexpectedEndOfStream);

            if (closingBoundary) _buffer.ReadTo(EpiloguePostfix);

            var result = _buffer.ReadTo(CRLF, LinearWhitespace);

            if (result.Invalid)
                return new ReadResult(result, ErrorInvalidCharactersFollowingBoundary);

            if (result.EndOfStream && !closingBoundary)
                return new ReadResult(result, ErrorUnexpectedEndOfStream);

            if (closingBoundary)
                return new ReadResult(_currentSection = MultipartSection.Epilogue, result);

            if (!_buffer.StartsWith(CRLF))
                return new ReadResult(_currentSection = MultipartSection.Headers, result);

            _buffer.ReadTo(CRLF);

            return new ReadResult(_currentSection = MultipartSection.Body, result);
        }

        private ReadResult ReadEpilogue(byte[] buffer, int offset, int count)
        {
            if (EndOfStream)
                return new ReadResult(MultipartSection.Epilogue, null);

            var result = _buffer.Read(buffer, offset, count, _boundaryLine, _boundary);

            if (result.Invalid)
                return new ReadResult(MultipartSection.Epilogue, 
                    result, ErrorBoundaryFoundAfterClosingBoundary);

            EndOfStream = result.EndOfStream;
            EndOfPart = result.EndOfSection;

            return new ReadResult(MultipartSection.Epilogue, result);
        }
    }
}