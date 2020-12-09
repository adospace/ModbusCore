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
        private readonly byte[] _buffer;
        private readonly int _offset;

        public ushort Length { get; private set; }

        public MessageBuffer()
        {
            _buffer = new byte[MAX_MESSAGE_LENGTH];
        }

        public MessageBuffer(MessageBuffer ownerBuffer, int offset)
        {
            _offset = offset;
            _buffer = ownerBuffer._buffer;
        }

        public byte this[int index]
        {
            get => _buffer[_offset + index];
            private set => _buffer[_offset + index] = value;
        }

        public void Clear() => Length = 0;

        public bool IsEmpty => Length == 0;

        public class MessageBufferWriter : IMessageBufferWriter
        {
            private readonly MessageBuffer _owner;

            public int Length => _owner.Length;

            public MessageBuffer Buffer => _owner;

            public MessageBufferWriter(MessageBuffer owner)
            {
                _owner = owner;
                _owner.Length = 0;
            }

            public void Push(byte value)
            {
                _owner[_owner.Length] = value;
                _owner.Length++;
            }

            public void Dispose()
            {
                _owner.Close();
            }

            public void Log(IPacketLogger packetLogger)
            {
                packetLogger.SendingPacket(new ReadOnlySpan<byte>(_owner._buffer, 0, _owner.Length));
            }
        }

        public class MessageBufferReader : IMessageBufferReader
        {
            private readonly MessageBuffer _owner;
            private readonly Stream _stream;

            public int Length => _owner.Length;

            public MessageBuffer Buffer => _owner;

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

            public void Log(IPacketLogger packetLogger)
            {
                packetLogger.ReceivedPacket(new ReadOnlySpan<byte>(_owner._buffer, 0, _owner.Length));
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
            => WriteToStream(stream, 0, Length);

        internal void WriteToStream(Stream stream, int offset, int length)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.Write(_buffer, offset, length);
        }

        internal Task WriteToStreamAsync(Stream stream, CancellationToken cancellationToken)
            => WriteToStreamAsync(stream, 0, Length, cancellationToken);

        internal Task WriteToStreamAsync(Stream stream, int offset, int length, CancellationToken cancellationToken)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return stream.WriteAsync(_buffer, offset, length, cancellationToken);
        }
    }
}