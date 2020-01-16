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

        public void WriteSingleCoil(int zeroBasedOffset, bool value)
        {
            SendWriteSingleCoilRequest(zeroBasedOffset, value);

            ReceiveWriteSingleCoilResponse(zeroBasedOffset);
        }

        public void WriteSingleRegister(int zeroBasedOffset, int value)
        {
            SendWriteSingleRegisterRequest(zeroBasedOffset, value);

            ReceiveWriteSingleRegisterResponse(zeroBasedOffset);
        }

        public void WriteMultipleCoils(int zeroBasedOffset, bool[] values)
        {
            Validate.Between(nameof(values) + ".Length", values.Length, 0, 0x07B0);

            SendWriteMultipleCoilsRequest(zeroBasedOffset, values);

            ReceiveWriteMultipleCoilsResponse(zeroBasedOffset, out var countOfValuesReturned);

            if (countOfValuesReturned != values.Length)
                throw new InvalidOperationException();
        }

        public void WriteMultipleRegisters(int zeroBasedOffset, int[] values)
        {
            Validate.Between(nameof(values) + ".Length", values.Length, 0, 0x07B0);

            SendWriteMultipleRegistersRequest(zeroBasedOffset, values);

            ReceiveWriteMultipleRegistersResponse(zeroBasedOffset, out var countOfValuesReturned);

            if (countOfValuesReturned != values.Length)
                throw new InvalidOperationException();
        }

        internal void ReceiveReadCoilsResponse(int zeroBasedOffset, int count)
            => ReceiveReadResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils);

        internal void ReceiveReadDiscreteInputsResponse(int zeroBasedOffset, int count)
            => ReceiveReadResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs);

        internal void ReceiveReadHoldingRegistersResponse(int zeroBasedOffset, int count)
            => ReceiveReadResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters);

        internal void ReceiveReadInputRegistersResponse(int zeroBasedOffset, int count)
            => ReceiveReadResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters);

        internal void ReceiveWriteSingleCoilResponse(int zeroBasedOffset)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.WriteSingleCoil | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteSingleCoil)
                throw new InvalidOperationException();

            if ((ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var value = messageBufferReader.PushFromStream() == 0xFF;

            messageBufferReader.PushFromStream();

            CheckCrcIsValidFromResponse(messageBufferReader);

            MemoryMap.OutputCoils[zeroBasedOffset] = value;
        }

        internal void ReceiveWriteMultipleCoilsResponse(int zeroBasedOffset, out int count)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.WriteMultipleCoils | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteMultipleCoils)
                throw new InvalidOperationException();

            if ((ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            count =  (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            CheckCrcIsValidFromResponse(messageBufferReader);
        }

        internal void ReceiveWriteSingleRegisterResponse(int zeroBasedOffset)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.WriteSingleRegister | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteSingleRegister)
                throw new InvalidOperationException();

            if ((ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var value = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            CheckCrcIsValidFromResponse(messageBufferReader);

            MemoryMap.OutputRegisters[zeroBasedOffset] = value;
        }

        internal void ReceiveWriteMultipleRegistersResponse(int zeroBasedOffset, out int count)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.WriteMultipleRegisters | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteMultipleRegisters)
                throw new InvalidOperationException();

            if ((ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            count = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            CheckCrcIsValidFromResponse(messageBufferReader);
        }

        internal void SendReadCoilsRequest(int zeroBasedOffset, int count)
                            => SendReadRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils);

        internal void SendReadDiscreteInputsRequest(int zeroBasedOffset, int count)
            => SendReadRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs);

        internal void SendReadHoldingRegistersRequest(int zeroBasedOffset, int count)
            => SendReadRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters);

        internal void SendReadInputRegistersRequest(int zeroBasedOffset, int count)
            => SendReadRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters);

        internal void SendWriteSingleCoilRequest(int zeroBasedOffset, bool value)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteSingleCoil);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)(value ? 0xFF : 0x00));
            messageBufferWriter.Push(0);

            AppendCrcToRequest(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        internal void SendWriteSingleRegisterRequest(int zeroBasedOffset, int value)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteSingleRegister);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((value >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((value >> 0) & 0xFF));

            AppendCrcToRequest(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        internal void SendWriteMultipleCoilsRequest(int zeroBasedOffset, params bool[] values)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteMultipleCoils);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));

            int count = values.Length;
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            values.CopyTo(messageBufferWriter);

            AppendCrcToRequest(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        internal void SendWriteMultipleRegistersRequest(int zeroBasedOffset, params int[] values)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteMultipleRegisters);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));

            int count = values.Length;
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            values.CopyTo(messageBufferWriter);

            AppendCrcToRequest(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

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

        private void ReceiveReadResponse(int zeroBasedOffset, int count, ModbusFunctionCode expectedFunctionCode)
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

        private void SendReadRequest(int zeroBasedOffset, int count, ModbusFunctionCode functionCode)
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