using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task ReadCoilsAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            await SendReadCoilsRequestAsync(zeroBasedOffset, count, cancellationToken);

            await ReceiveReadCoilsResponseAsync(zeroBasedOffset, count, cancellationToken);
        }

        public void ReadDiscreteInputs(int zeroBasedOffset, int count)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            SendReadDiscreteInputsRequest(zeroBasedOffset, count);

            ReceiveReadDiscreteInputsResponse(zeroBasedOffset, count);
        }

        public async Task ReadDiscreteInputsAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            await SendReadDiscreteInputsRequestAsync(zeroBasedOffset, count, cancellationToken);

            await ReceiveReadDiscreteInputsResponseAsync(zeroBasedOffset, count, cancellationToken);
        }

        public void ReadHoldingRegisters(int zeroBasedOffset, int count)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            SendReadHoldingRegistersRequest(zeroBasedOffset, count);

            ReceiveReadHoldingRegistersResponse(zeroBasedOffset, count);
        }

        public async Task ReadHoldingRegistersAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            await SendReadHoldingRegistersRequestAsync(zeroBasedOffset, count, cancellationToken);

            await ReceiveReadHoldingRegistersResponseAsync(zeroBasedOffset, count, cancellationToken);
        }

        public void ReadInputRegisters(int zeroBasedOffset, int count)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            SendReadInputRegistersRequest(zeroBasedOffset, count);

            ReceiveReadInputRegistersResponse(zeroBasedOffset, count);
        }

        public async Task ReadInputRegistersAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken)
        {
            Validate.Between(nameof(count), count, 0, 2000);
            Validate.Between(nameof(zeroBasedOffset) + " + " + nameof(zeroBasedOffset), zeroBasedOffset + count, 0, ushort.MaxValue);
            Validate.Between(nameof(zeroBasedOffset), zeroBasedOffset, 0, count - zeroBasedOffset);

            await SendReadInputRegistersRequestAsync(zeroBasedOffset, count, cancellationToken);

            await ReceiveReadInputRegistersResponseAsync(zeroBasedOffset, count, cancellationToken);
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

            ReceiveWriteMultipleCoilsResponse(zeroBasedOffset);//, out var countOfValuesReturned);

            //if (countOfValuesReturned != values.Length)
            //    throw new InvalidOperationException();
        }

        public void WriteMultipleRegisters(int zeroBasedOffset, int[] values)
        {
            Validate.Between(nameof(values) + ".Length", values.Length, 0, 0x07B0);

            SendWriteMultipleRegistersRequest(zeroBasedOffset, values);

            ReceiveWriteMultipleRegistersResponse(zeroBasedOffset);//, out var countOfValuesReturned);

            //if (countOfValuesReturned != values.Length)
            //    throw new InvalidOperationException();
        }

        internal void ReceiveReadCoilsResponse(int zeroBasedOffset, int count)
            => ReceiveReadResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils);

        internal Task ReceiveReadCoilsResponseAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken)
            => ReceiveReadResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils, cancellationToken);

        internal void ReceiveReadDiscreteInputsResponse(int zeroBasedOffset, int count)
            => ReceiveReadResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs);

        internal Task ReceiveReadDiscreteInputsResponseAsync(int zeroBasedOffset, int count, CancellationToken cancellation)
            => ReceiveReadResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs, cancellation);

        internal void ReceiveReadHoldingRegistersResponse(int zeroBasedOffset, int count)
            => ReceiveReadResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters);

        internal Task ReceiveReadHoldingRegistersResponseAsync(int zeroBasedOffset, int count, CancellationToken cancellation)
            => ReceiveReadResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters, cancellation);

        internal void ReceiveReadInputRegistersResponse(int zeroBasedOffset, int count)
            => ReceiveReadResponse(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters);

        internal Task ReceiveReadInputRegistersResponseAsync(int zeroBasedOffset, int count, CancellationToken cancellation)
            => ReceiveReadResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters, cancellation);

        internal void ReceiveWriteSingleCoilResponse(int zeroBasedOffset)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushByteFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.WriteSingleCoil | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushByteFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteSingleCoil)
                throw new InvalidOperationException();

            if ((ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var value = messageBufferReader.PushByteFromStream() == 0xFF;

            messageBufferReader.PushByteFromStream();

            CheckCrcIsValidFromResponse(messageBufferReader);

            MemoryMap.OutputCoils[zeroBasedOffset] = value;
        }

        internal async Task ReceiveWriteSingleCoilResponseAsync(int zeroBasedOffset, CancellationToken cancellationToken)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            if (functionCode == ((byte)ModbusFunctionCode.WriteSingleCoil | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)await messageBufferReader.PushByteFromStreamAsync(cancellationToken);
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteSingleCoil)
                throw new InvalidOperationException();

            if ((ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) + (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var value = await messageBufferReader.PushByteFromStreamAsync(cancellationToken) == 0xFF;

            await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            CheckCrcIsValidFromResponse(messageBufferReader);

            MemoryMap.OutputCoils[zeroBasedOffset] = value;
        }

        internal void ReceiveWriteMultipleCoilsResponse(int zeroBasedOffset)//, out int count)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushByteFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.WriteMultipleCoils | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushByteFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteMultipleCoils)
                throw new InvalidOperationException();

            if ((ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var count =  (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));

            CheckCrcIsValidFromResponse(messageBufferReader);
        }

        internal void ReceiveWriteSingleRegisterResponse(int zeroBasedOffset)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushByteFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.WriteSingleRegister | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushByteFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteSingleRegister)
                throw new InvalidOperationException();

            if ((ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var value = (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));

            CheckCrcIsValidFromResponse(messageBufferReader);

            MemoryMap.OutputRegisters[zeroBasedOffset] = value;
        }

        internal async Task ReceiveWriteSingleRegisterResponseAsync(int zeroBasedOffset, CancellationToken cancellationToken)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            if (functionCode == ((byte)ModbusFunctionCode.WriteSingleRegister | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)await messageBufferReader.PushByteFromStreamAsync(cancellationToken);
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteSingleRegister)
                throw new InvalidOperationException();

            if ((ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) + (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var value = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) + (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));

            CheckCrcIsValidFromResponse(messageBufferReader);

            MemoryMap.OutputRegisters[zeroBasedOffset] = value;
        }

        internal void ReceiveWriteMultipleRegistersResponse(int zeroBasedOffset)//, out int count)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushByteFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.WriteMultipleRegisters | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushByteFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteMultipleRegisters)
                throw new InvalidOperationException();

            if ((ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var count = (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));

            CheckCrcIsValidFromResponse(messageBufferReader);
        }

        internal void SendReadCoilsRequest(int zeroBasedOffset, int count)
            => SendReadRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils);

        internal Task SendReadCoilsRequestAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken)
            => SendReadRequestAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils, cancellationToken);

        internal void SendReadDiscreteInputsRequest(int zeroBasedOffset, int count)
            => SendReadRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs);

        internal Task SendReadDiscreteInputsRequestAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken)
            => SendReadRequestAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs, cancellationToken);

        internal void SendReadHoldingRegistersRequest(int zeroBasedOffset, int count)
            => SendReadRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters);

        internal Task SendReadHoldingRegistersRequestAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken)
            => SendReadRequestAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters, cancellationToken);

        internal void SendReadInputRegistersRequest(int zeroBasedOffset, int count)
            => SendReadRequest(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters);
        
        internal Task SendReadInputRegistersRequestAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken)
            => SendReadRequestAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters, cancellationToken);

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

        internal async Task SendWriteSingleCoilRequestAsync(int zeroBasedOffset, bool value, CancellationToken cancellationToken)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteSingleCoil);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)(value ? 0xFF : 0x00));
            messageBufferWriter.Push(0);

            AppendCrcToRequest(messageBufferWriter);

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken);
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

        internal async Task SendWriteSingleRegisterRequestAsync(int zeroBasedOffset, int value, CancellationToken cancellationToken)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteSingleRegister);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((value >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((value >> 0) & 0xFF));

            AppendCrcToRequest(messageBufferWriter);

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken);
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

        internal async Task SendWriteMultipleCoilsRequestAsync(int zeroBasedOffset, bool[] values, CancellationToken cancellationToken)
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

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken);
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

        internal async Task SendWriteMultipleRegistersRequestAsync(int zeroBasedOffset, int[] values, CancellationToken cancellationToken)
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

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken);
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
            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushByteFromStream();

            if (functionCode == ((byte)expectedFunctionCode | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushByteFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)expectedFunctionCode)
                throw new InvalidOperationException();

            var byteCount = messageBufferReader.PushByteFromStream();

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
                    MemoryMap.InputRegisters.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, count);
                    break;
            }
        }

        private async Task ReceiveReadResponseAsync(int zeroBasedOffset, int count, ModbusFunctionCode expectedFunctionCode, CancellationToken cancellationToken)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);
            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            byte functionCode = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            if (functionCode == ((byte)expectedFunctionCode | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)await messageBufferReader.PushByteFromStreamAsync(cancellationToken);
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)expectedFunctionCode)
                throw new InvalidOperationException();

            var byteCount = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            await messageBufferReader.PushFromStreamAsync(byteCount, cancellationToken);

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
                    MemoryMap.InputRegisters.CopyFrom(
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

        private async Task SendReadRequestAsync(int zeroBasedOffset, int count, ModbusFunctionCode functionCode, CancellationToken cancellationToken)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)functionCode);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            AppendCrcToRequest(messageBufferWriter);

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken);
        }
    }
}