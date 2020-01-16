using System;

namespace ModbusCore
{
    public class MessageBufferSpan
    {
        private readonly MessageBuffer _messageBuffer;

        public MessageBufferSpan(MessageBuffer messageBuffer, ushort startIndex, ushort bytesCount)
        {
            _messageBuffer = messageBuffer;
            StartIndex = startIndex;
            BytesCount = bytesCount;
        }

        public byte this[int index]
        {
            get
            {
                if (index < 0 || StartIndex + index >= _messageBuffer.Length)
                    throw new ArgumentOutOfRangeException();

                return _messageBuffer[StartIndex + index];
            }
        }

        public ushort StartIndex { get; }
        public ushort BytesCount { get; }

        public ushort Length => BytesCount;
    }
}