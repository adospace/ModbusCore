using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public class MessageBuffer
    {
        public const int MAX_MESSAGE_LENGTH = 256;
        private readonly byte[] _buffer = new byte[MAX_MESSAGE_LENGTH];

        public ushort Length { get; private set; }

        public byte this[int index]
        {
            get => _buffer[index];
            private set => _buffer[index] = value;
        }

        public void Clear() => Length = 0;

        public bool IsEmpty => Length == 0;

        public class MessageBufferWriter : IMessageBufferWriter
        {
            private readonly MessageBuffer _owner;

            public int Length => _owner.Length;

            public MessageBufferWriter(MessageBuffer owner)
            {
                _owner = owner;
                _owner.Length = 0;
            }

            public void Push(byte value)
            {
                _owner._buffer[_owner.Length] = value;
                _owner.Length++;
            }

            public void Dispose()
            {
                _owner.Close();
            }
        }

        public class MessageBufferReader : IMessageBufferReader
        {
            private readonly MessageBuffer _owner;
            private readonly Stream _stream;

            public int Length => _owner.Length;

            public MessageBufferReader(MessageBuffer owner, Stream stream)
            {
                _owner = owner;
                _stream = stream;
                _owner.Length = 0;
            }

            public void Dispose()
            {
                _owner.Close();
            }

            public byte PushByteFromStream()
            {
                PushFromStream(1);
                return _owner._buffer[_owner.Length - 1];
            }

            public void PushFromStream(int byteCountToReadFromStreamAndPushToBuffer)
            {
                while (byteCountToReadFromStreamAndPushToBuffer > 0)
                {
                    var readBytes = _stream.Read(_owner._buffer, Length, byteCountToReadFromStreamAndPushToBuffer);
                    _owner.Length += (ushort)readBytes;
                    byteCountToReadFromStreamAndPushToBuffer -= readBytes;
                }
            }

            public async Task<byte> PushByteFromStreamAsync(CancellationToken cancellationToken)
            {
                await PushFromStreamAsync(1, cancellationToken);
                return _owner._buffer[_owner.Length - 1];
            }

            public async Task PushFromStreamAsync(int byteCountToReadFromStreamAndPushToBuffer, CancellationToken cancellationToken)
            {
                while (byteCountToReadFromStreamAndPushToBuffer > 0)
                {
                    var readBytes = await _stream.ReadAsync(_owner._buffer, Length, byteCountToReadFromStreamAndPushToBuffer, cancellationToken);
                    _owner.Length += (ushort)readBytes;
                    byteCountToReadFromStreamAndPushToBuffer -= readBytes;
                }
            }
        }

        private MessageBufferWriter? _writer = null;

        public IMessageBufferWriter BeginWrite()
        {
            if (_writer != null)
                throw new InvalidOperationException();

            return _writer = new MessageBufferWriter(this);
        }

        private MessageBufferReader? _reader = null;

        public IMessageBufferReader BeginRead(Stream stream)
        {
            if (_reader != null)
                throw new InvalidOperationException();

            return _reader = new MessageBufferReader(this, stream);
        }

        private void Close()
        {
            _writer = null;
            _reader = null;
        }

        internal void WriteToStream(Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.Write(_buffer, 0, Length);
        }

        internal Task WriteToStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return stream.WriteAsync(_buffer, 0, Length, cancellationToken);
        }
    }
}