using System;
using System.IO;

namespace ModbusCore
{
    public class ModbusRTUSlave : ModbusDevice, IModbusSlave
    {
        private readonly MessageBuffer _messageBuffer = new MessageBuffer();

        public ModbusRTUSlave(ModbusMemoryMap memoryMap, Stream stream, byte address)
            : base(memoryMap, stream, address)
        {
        }

        public void HandleAnyRequest()
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            var functionCode = (ModbusFunctionCode)messageBufferReader.PushFromStream();

            switch (functionCode)
            {
                case ModbusFunctionCode.ReadDiscreteInputs:
                    {
                        HandleReadRequest(messageBufferReader, out var offset, out var count);
                        HandleReadDiscreteInputsResponse(offset, count);
                    }
                    break;

                case ModbusFunctionCode.ReadCoils:
                    {
                        HandleReadRequest(messageBufferReader, out var offset, out var count);
                        HandleReadCoilsResponse(offset, count);
                    }
                    return;

                case ModbusFunctionCode.WriteSingleCoil:
                    {
                        HandleWriteSingleCoilRequest(messageBufferReader, out var offset);
                        HandleWriteSingleCoilResponse(offset);
                    }
                    return;

                case ModbusFunctionCode.WriteMultipleCoils:
                    {
                        HandleWriteMultipleCoilsRequest(messageBufferReader, out var offset, out var count);
                        HandleWriteMultipleCoilsResponse(offset, count);
                    }
                    return;

                case ModbusFunctionCode.ReadInputRegisters:
                    {
                        HandleReadRequest(messageBufferReader, out var offset, out var count);
                        HandleReadInputRegistersResponse(offset, count);
                    }
                    return;

                case ModbusFunctionCode.ReadHoldingRegisters:
                    {
                        HandleReadRequest(messageBufferReader, out var offset, out var count);
                        HandleReadHoldingRegistersResponse(offset, count);
                    }
                    return;

                case ModbusFunctionCode.WriteSingleRegister:
                    {
                        HandleWriteSingleRegisterRequest(messageBufferReader, out var offset);
                        HandleWriteSingleRegisterResponse(offset);
                    }
                    return;

                case ModbusFunctionCode.WriteMultipleRegisters:
                    {
                        HandleWriteMultipleRegistersRequest(messageBufferReader, out var offset, out var count);
                        HandleWriteMultipleRegistersResponse(offset, count);
                    }
                    return;

                case ModbusFunctionCode.ReadWriteMultipleRegisters:
                    break;

                case ModbusFunctionCode.MaskWriteRegister:
                    break;

                case ModbusFunctionCode.ReadFiFoQueue:
                    break;

                case ModbusFunctionCode.ReadFileRecord:
                    break;

                case ModbusFunctionCode.WriteFileRecord:
                    break;

                case ModbusFunctionCode.ReadExceptionStatus:
                    break;

                case ModbusFunctionCode.Diagnostic:
                    break;

                case ModbusFunctionCode.GetComEventCounter:
                    break;

                case ModbusFunctionCode.GetComEventLog:
                    break;

                case ModbusFunctionCode.ReportServerID:
                    break;

                case ModbusFunctionCode.ReadDeviceIdentification:
                    break;
            }

            throw new NotImplementedException($"Function '{functionCode}' is not implemented");
        }

        internal void HandleReadCoilsRequest(out int zeroBasedOffset, out int count)
            => HandleReadRequest(ModbusFunctionCode.ReadCoils, out zeroBasedOffset, out count);

        internal void HandleReadCoilsResponse(int zeroBasedOffset, int count)
            => HandleBitArrayResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils);

        internal void HandleReadHoldingRegistersRequest(out int zeroBasedOffset, out int count)
            => HandleReadRequest(ModbusFunctionCode.ReadHoldingRegisters, out zeroBasedOffset, out count);

        internal void HandleReadHoldingRegistersResponse(int zeroBasedOffset, int count)
            => HandleRegisterArrayResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters);

        internal void HandleReadDiscreteInputsRequest(out int zeroBasedOffset, out int count)
                    => HandleReadRequest(ModbusFunctionCode.ReadDiscreteInputs, out zeroBasedOffset, out count);

        internal void HandleReadDiscreteInputsResponse(int zeroBasedOffset, int count)
            => HandleBitArrayResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs);

        internal void HandleReadInputRegistersRequest(out int zeroBasedOffset, out int count)
            => HandleReadRequest(ModbusFunctionCode.ReadInputRegisters, out zeroBasedOffset, out count);

        internal void HandleReadInputRegistersResponse(int zeroBasedOffset, int count)
            => HandleRegisterArrayResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters);

        internal void HandleWriteSingleCoilRequest(out int zeroBasedOffset)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushFromStream() != (byte)ModbusFunctionCode.WriteSingleCoil)
                throw new InvalidOperationException();

            HandleWriteSingleCoilRequest(messageBufferReader, out zeroBasedOffset);
        }

        internal void HandleWriteSingleCoilRequest(IMessageBufferReader messageBufferReader, out int zeroBasedOffset)
        {
            zeroBasedOffset = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            var value = messageBufferReader.PushFromStream() == 0xFF;

            if (messageBufferReader.PushFromStream() != 0x00)
                throw new InvalidOperationException();

            CheckCrcIsValidFromRequest(messageBufferReader);

            MemoryMap.OutputCoils[zeroBasedOffset] = value;
        }

        internal void HandleWriteSingleCoilResponse(int zeroBasedOffset)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteSingleCoil);

            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)(MemoryMap.OutputCoils[zeroBasedOffset] ? 0xFF : 0x00));
            messageBufferWriter.Push(0);

            AppendCrcToResponse(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        internal void HandleWriteSingleRegisterRequest(out int zeroBasedOffset)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushFromStream() != (byte)ModbusFunctionCode.WriteSingleRegister)
                throw new InvalidOperationException();

            HandleWriteSingleRegisterRequest(messageBufferReader, out zeroBasedOffset);
        }

        private void HandleWriteSingleRegisterRequest(IMessageBufferReader messageBufferReader, out int zeroBasedOffset)
        {
            zeroBasedOffset = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));
            var value = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            CheckCrcIsValidFromRequest(messageBufferReader);

            MemoryMap.OutputRegisters[zeroBasedOffset] = value;
        }

        internal void HandleWriteSingleRegisterResponse(int zeroBasedOffset)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteSingleRegister);

            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((MemoryMap.OutputRegisters[zeroBasedOffset] >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((MemoryMap.OutputRegisters[zeroBasedOffset] >> 0) & 0xFF));

            AppendCrcToResponse(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        internal void HandleWriteMultipleCoilsRequest(out int zeroBasedOffset, out int countOfValues)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushFromStream() != (byte)ModbusFunctionCode.WriteMultipleCoils)
                throw new InvalidOperationException();

            HandleWriteMultipleCoilsRequest(messageBufferReader, out zeroBasedOffset, out countOfValues);
        }

        private void HandleWriteMultipleCoilsRequest(IMessageBufferReader messageBufferReader, out int zeroBasedOffset, out int countOfValues)
        {
            zeroBasedOffset = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            countOfValues = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            var byteCount = messageBufferReader.PushFromStream();

            messageBufferReader.PushFromStream(byteCount);

            CheckCrcIsValidFromRequest(messageBufferReader);

            //update the memory map
            MemoryMap.OutputCoils.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, countOfValues);
        }

        internal void HandleWriteMultipleCoilsResponse(int zeroBasedOffset, int count)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteMultipleCoils);

            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            AppendCrcToResponse(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        internal void HandleWriteMultipleRegistersRequest(out int zeroBasedOffset, out int countOfValues)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushFromStream() != (byte)ModbusFunctionCode.WriteMultipleRegisters)
                throw new InvalidOperationException();

            HandleWriteMultipleRegistersRequest(messageBufferReader, out zeroBasedOffset, out countOfValues);
        }

        internal void HandleWriteMultipleRegistersRequest(IMessageBufferReader messageBufferReader, out int zeroBasedOffset, out int countOfValues)
        {
            zeroBasedOffset = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            countOfValues = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            var byteCount = messageBufferReader.PushFromStream();

            messageBufferReader.PushFromStream(byteCount);

            CheckCrcIsValidFromRequest(messageBufferReader);

            //update the memory map
            MemoryMap.OutputRegisters.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, countOfValues);
        }

        internal void HandleWriteMultipleRegistersResponse(int zeroBasedOffset, int count)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteMultipleRegisters);

            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            AppendCrcToResponse(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        private void AppendCrcToResponse(IMessageBufferWriter messageBufferWriter)
        {
            var crc = CrcUtils.CRC16(_messageBuffer);

            // Write the high nibble of the CRC
            messageBufferWriter.Push((byte)(((crc & 0xFF00) >> 8) & 0xFF));

            // Write the low nibble of the CRC
            messageBufferWriter.Push((byte)((crc & 0x00FF) & 0xFF));
        }

        private void CheckCrcIsValidFromRequest(IMessageBufferReader messageBufferReader)
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

        private void HandleBitArrayResponse(int zeroBasedOffset, int count, ModbusFunctionCode functionCode)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)functionCode);

            if (functionCode == ModbusFunctionCode.ReadCoils)
            {
                MemoryMap.OutputCoils.CopyTo(messageBufferWriter, zeroBasedOffset, count);
            }
            else
            {
                MemoryMap.InputCoils.CopyTo(messageBufferWriter, zeroBasedOffset, count);
            }

            AppendCrcToResponse(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        private void HandleRegisterArrayResponse(int zeroBasedOffset, int count, ModbusFunctionCode functionCode)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)functionCode);

            if (functionCode == ModbusFunctionCode.ReadHoldingRegisters)
            {
                MemoryMap.OutputRegisters.CopyTo(messageBufferWriter, zeroBasedOffset, count);
            }
            else
            {
                MemoryMap.InputRegisters.CopyTo(messageBufferWriter, zeroBasedOffset, count);
            }

            AppendCrcToResponse(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        private void HandleReadRequest(ModbusFunctionCode functionCode, out int zeroBasedOffset, out int count)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushFromStream() != (byte)functionCode)
                throw new InvalidOperationException();

            HandleReadRequest(messageBufferReader, out zeroBasedOffset, out count);
        }

        private void HandleReadRequest(IMessageBufferReader messageBufferReader, out int zeroBasedOffset, out int count)
        {
            zeroBasedOffset = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));
            count = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            CheckCrcIsValidFromRequest(messageBufferReader);
        }
    }
}