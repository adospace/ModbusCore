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

        public void ReadDiscreteInputs(int zeroBasedOffset, int count)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            SendReadDiscreteInputsRequest(zeroBasedOffset, count);

            ReceiveReadDiscreteInputsResponse(zeroBasedOffset, count);
        }

        public void ReadHoldingRegisters(int zeroBasedOffset, int count)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            SendReadHoldingRegistersRequest(zeroBasedOffset, count);

            ReceiveReadHoldingRegistersResponse(zeroBasedOffset, count);
        }

        public void ReadInputRegisters(int zeroBasedOffset, int count)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            SendReadInputRegistersRequest(zeroBasedOffset, count);

            ReceiveReadInputRegistersResponse(zeroBasedOffset, count);
        }

        internal void ReceiveReadCoilsResponse(int zeroBasedOffset, int count)
            => ReceiveBitArrayResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils);

        internal void ReceiveReadDiscreteInputsResponse(int zeroBasedOffset, int count)
            => ReceiveBitArrayResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs);

        internal void ReceiveReadHoldingRegistersResponse(int zeroBasedOffset, int count)
            => ReceiveBitArrayResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters);

        internal void ReceiveReadInputRegistersResponse(int zeroBasedOffset, int count)
            => ReceiveBitArrayResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters);

        internal void SendReadCoilsRequest(int zeroBasedOffset, int count)
            => SendBitArrayRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils);

        internal void SendReadDiscreteInputsRequest(int zeroBasedOffset, int count)
            => SendBitArrayRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs);

        internal void SendReadHoldingRegistersRequest(int zeroBasedOffset, int count)
            => SendBitArrayRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters);

        internal void SendReadInputRegistersRequest(int zeroBasedOffset, int count)
            => SendBitArrayRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters);

        private void AppendCrcToRequest(IMessageBufferWriter messageBufferWriter)
        {
            var crc = CrcUtils.CRC16(_messageBuffer);

            // Write the high nibble of the CRC
            messageBufferWriter.Push((byte)(((crc & 0xFF00) >> 8) & 0xFF));

            // Write the low nibble of the CRC
            messageBufferWriter.Push((byte)((crc & 0x00FF) & 0xFF));
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

        private void ReceiveBitArrayResponse(int zeroBasedOffset, int count, ModbusFunctionCode expectedFunctionCode)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushFromStream();

            if (functionCode == ((byte)expectedFunctionCode | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)expectedFunctionCode)
                throw new InvalidOperationException();

            var byteCount = messageBufferReader.PushFromStream();

            messageBufferReader.PushFromStream(byteCount);

            CheckCrcIsValidFromResponse(messageBufferReader);

            //update the memory map
            switch (expectedFunctionCode)
            {
                case ModbusFunctionCode.ReadCoils:
                    MemoryMap.OutputCoils.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, count);
                    break;
                case ModbusFunctionCode.ReadDiscreteInputs:
                    MemoryMap.InputCoils.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, count);
                    break;
                case ModbusFunctionCode.ReadHoldingRegisters:
                    MemoryMap.OutputRegisters.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, count);
                    break;
                case ModbusFunctionCode.ReadInputRegisters:
                    MemoryMap.OutputRegisters.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, count);
                    break;
            }
        }
        
        private void SendBitArrayRequest(int zeroBasedOffset, int count, ModbusFunctionCode functionCode)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)functionCode);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            AppendCrcToRequest(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }
    }
}