using System;
using System.IO;

namespace ModbusCore
{
    public class ModbusRTUMaster : ModbusRTUDevice, IModbusMaster
    {
        private readonly MessageBuffer _messageBuffer = new MessageBuffer();

        public ModbusRTUMaster(ModbusMemoryMap memoryMap, Stream stream, byte address)
            : base(memoryMap, stream, address)
        {
        }

        public void ReadCoils(int zeroBasedOffset, int count)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            SendReadCoilsRequest(zeroBasedOffset, count);

            ReceiveReadCoilsResponse(zeroBasedOffset, count);
        }

        internal void ReceiveReadCoilsResponse(int zeroBasedOffset, int count)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.ReadCoils | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.ReadCoils)
                throw new InvalidOperationException();

            var byteCount = messageBufferReader.PushFromStream();

            //read coils status
            messageBufferReader.PushFromStream(byteCount);

            CheckCrcIsValidFromResponse(messageBufferReader);

            //update the memory map
            MemoryMap.InputCoils.CopyFrom(
                new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, count);
        }

        internal void SendReadCoilsRequest(int zeroBasedOffset, int count)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.ReadCoils);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            AppendCrcToRequest(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        private void CheckCrcIsValidFromResponse(IMessageBufferReader messageBufferReader)
        {
            var messageCrc = CrcUtils.CRC16(_messageBuffer);

            //check CRC
            messageBufferReader.PushFromStream(2);

            if (_messageBuffer[_messageBuffer.Length - 2] != (byte)(((messageCrc & 0xFF00) >> 8) & 0xFF) ||
                _messageBuffer[_messageBuffer.Length - 1] != (byte)((messageCrc & 0x00FF) & 0xFF))
            {
                throw new ModbusInvalidCRCException();
            }
        }

        private void AppendCrcToRequest(IMessageBufferWriter messageBufferWriter)
        {
            var crc = CrcUtils.CRC16(_messageBuffer);

            // Write the high nibble of the CRC
            messageBufferWriter.Push((byte)(((crc & 0xFF00) >> 8) & 0xFF));

            // Write the low nibble of the CRC
            messageBufferWriter.Push((byte)((crc & 0x00FF) & 0xFF));
        }
    }
}