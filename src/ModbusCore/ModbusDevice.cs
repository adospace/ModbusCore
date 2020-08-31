using System;
using System.IO;

namespace ModbusCore
{
    public abstract class ModbusDevice
    {
        protected readonly MessageBuffer _messageBuffer = new MessageBuffer();

        protected ModbusDevice(ModbusMemoryMap memoryMap, Stream stream, byte address, IPacketLogger? packetLogger = null)
        {
            MemoryMap = memoryMap ?? throw new ArgumentNullException(nameof(memoryMap));
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            Address = address;
            PacketLogger = packetLogger;
        }

        public ModbusMemoryMap MemoryMap { get; }
        public Stream Stream { get; }
        public byte Address { get; }
        public IPacketLogger? PacketLogger { get; }

        protected void AppendCrc(IMessageBufferWriter messageBufferWriter)
        {
            var crc = CrcUtils.CRC16(_messageBuffer);

            // Write the high nibble of the CRC
            messageBufferWriter.Push((byte)(((crc & 0xFF00) >> 8) & 0xFF));

            // Write the low nibble of the CRC
            messageBufferWriter.Push((byte)((crc & 0x00FF) & 0xFF));

            if (PacketLogger != null)
            {
                messageBufferWriter.Log(PacketLogger);
            }
        }

        protected void CheckCrcIsValid(IMessageBufferReader messageBufferReader)
        {
            var messageCrc = CrcUtils.CRC16(_messageBuffer);

            //check CRC
            messageBufferReader.PushFromStream(2);

            if (PacketLogger != null)
            {
                messageBufferReader.Log(PacketLogger);
            }

            if (_messageBuffer[_messageBuffer.Length - 2] != (byte)(((messageCrc & 0xFF00) >> 8) & 0xFF) ||
                _messageBuffer[_messageBuffer.Length - 1] != (byte)((messageCrc & 0x00FF) & 0xFF))
            {
                throw new ModbusInvalidCRCException($"Invalid CRC: expected {(byte)(((messageCrc & 0xFF00) >> 8) & 0xFF):X2} {(byte)(messageCrc & 0x00FF & 0xFF):X2} received {_messageBuffer[_messageBuffer.Length - 2]:X2} {_messageBuffer[_messageBuffer.Length - 1]:X2}");
            }
        }
    }
}