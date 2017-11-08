using System;
using System.IO;
using Graphite.Linq;

namespace TestHarness
{
    public class DummyStream : Stream
    {
        private static readonly Random Random = new Random();
        private int _size;

        public DummyStream() { }

        public DummyStream(int size)
        {
            _size = size;
        }

        public int Checksum { get; private set; }
        public override long Length => _size;
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek { get; } = false;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_size == 0) return 0;
            var write = Math.Min(_size, count);
            _size -= write;
            var bytes = new byte[write];
            Random.NextBytes(bytes);
            bytes.CopyTo(buffer, offset);
            CalculateChecksum(bytes, 0, write);

            return write;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _size += count;
            CalculateChecksum(buffer, offset, count);
        }

        private void CalculateChecksum(byte[] buffer, int offset, int count)
        {
            offset.To(count).ForEach(x => Checksum = Checksum ^ buffer[x]);
        }

        public override void Flush() =>
            throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}
