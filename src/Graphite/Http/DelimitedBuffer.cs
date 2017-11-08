using System;
using System.IO;
using System.Linq;
using Graphite.Extensions;

namespace Graphite.Http
{
    public class DelimitedBuffer
    {
        public const int DefaultBufferSize = 1024;

        private readonly byte[] _buffer;
        private readonly Stream _stream;
        private bool _end;
        private int _size;
        private int _offset;

        public DelimitedBuffer(Stream stream,
            int bufferSize = DefaultBufferSize)
        {
            if (bufferSize < 1) throw new ArgumentException(
                "Buffer size must be greater than one.",
                nameof(bufferSize));

            _buffer = new byte[bufferSize];
            _stream = stream;
        }

        public bool BeginingOfStream { get; private set; } = true;

        public bool StartsWith(byte[] compare)
        {
            if (_size < compare.Length) Fill();

            return _size >= compare.Length && _buffer.ContainsSequenceAt(compare, _offset);
        }

        public ReadResult Read(byte[] buffer, int offset, int count,
            params byte[][] invalidTokens)
        {
            return ReadTo(buffer, offset, count, null, invalidTokens);
        }

        public ReadResult ReadTo(byte[] delimiter, params byte[] validBytes)
        {
            return ReadTo(delimiter, (b, o, c) => b.OnlyContains(validBytes, o, c), 1);
        }

        public ReadResult ReadTo(byte[] buffer, int offset, int count, 
            byte[] delimiter, params byte[][] invalidTokens)
        {
            return ReadTo(buffer, offset, count, delimiter, 
                (b, o, c) => !invalidTokens.Any(x => b.ContainsSequence(x, o, c)),
                invalidTokens.Any() ? invalidTokens.Max(x => x.Length) : 0);
        }

        public class ReadResult
        {
            public ReadResult(int read, bool endOfSection, bool endOfStream, bool invalid = false)
            {
                Read = read;
                EndOfSection = endOfSection;
                EndOfStream = endOfStream;
                Invalid = invalid;
            }

            public int Read { get; }
            public bool EndOfSection { get; }
            public bool EndOfStream { get; }
            public bool Invalid { get; }
        }
        
        private ReadResult ReadTo(byte[] delimiter, Func<byte[], int, int, bool> validate, int minimumPadding)
        {
            var read = 0;
            while (true)
            {
                var result = ReadTo(null, 0, DefaultBufferSize, delimiter, validate, minimumPadding);
                read += result.Read;
                if (result.Read == 0 || result.Invalid || result.EndOfSection || result.EndOfStream)
                    return new ReadResult(read, result.EndOfSection, 
                        result.EndOfStream, result.Invalid);
            }
        }
        
        private ReadResult ReadTo(byte[] buffer, int offset, int count, byte[] delimiter,
            Func<byte[], int, int, bool> validate, int minimumPadding)
        {
            if (offset < 0) throw new ArgumentException($"Offset must be greater than zero but was {offset}.");
            if (count < 1) throw new ArgumentException($"Count must be greater than 1 but was {count}.");
            if (_buffer.Length < minimumPadding)
                throw new ArgumentException("Buffer must be greater than or equal to the minimum padding.");

            minimumPadding = Math.Max(Math.Max(delimiter?.Length ?? 0, minimumPadding) - 1, 0);

            if (_size <= minimumPadding)
            {
                Fill();
                if (_end && _size == 0) return new ReadResult(0, true, true);
            }

            var maxSize = Math.Min(count, _size);

            var delimiterIndex = _buffer.IndexOfSequence(delimiter, _offset, maxSize);

            if (delimiterIndex == 0)
            {
                ShiftOffset(delimiter.Length);
                return new ReadResult(0, true, _end);
            }

            var readSize = delimiterIndex > 0
                ? delimiterIndex
                : Math.Min(maxSize, _size - minimumPadding);

            if (readSize <= 0) readSize = maxSize;

            if (!(validate?.Invoke(_buffer, _offset, readSize) ?? true))
                return new ReadResult(0, false, _end, true);

            if (buffer != null)
                Array.Copy(_buffer, _offset, buffer, offset, readSize);

            var endOfSection = _offset + readSize == delimiterIndex || (_end && _size - readSize == 0);

            ShiftOffset(readSize);
            if (endOfSection && delimiterIndex >= 0) ShiftOffset(delimiter.Length);

            return new ReadResult(readSize, endOfSection, _end);
        }

        private void ShiftOffset(int count)
        {
            BeginingOfStream = false;
            _size -= count;
            _offset = _size == 0 ? 0 : _offset + count;
        }

        private void Fill()
        {
            if (_end) return;

            if (_size > 0)
            {
                Array.Copy(_buffer, _offset, _buffer, 0, _size);
                _offset = 0;
            }
            
            var maxLength = _buffer.Length - _size;

            if (maxLength == 0) return;

            var bytesRead = _stream.Read(_buffer, _size, maxLength);

            if (bytesRead == 0) _end = true;

            _size += bytesRead;
        }
    }
}