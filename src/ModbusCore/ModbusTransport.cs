using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public abstract class ModbusTransport
    {
        internal class Request
        {
            public Request(int offset, int count)
            {
                Offset = offset;
                Count = count;
            }

            public int Offset { get; }
            public int Count { get; }
        }

        protected readonly MessageBuffer _messageBuffer = new MessageBuffer();

        public ModbusMemoryMap MemoryMap { get; }
        public Stream Stream { get; }
        public byte Address { get; }
        public IPacketLogger? PacketLogger { get; }

        protected ModbusTransport(ModbusMemoryMap memoryMap, Stream stream, byte address, IPacketLogger? packetLogger = null)
        {
            MemoryMap = memoryMap;
            Stream = stream;
            Address = address;
            PacketLogger = packetLogger;
        }

        protected virtual void OnBeginReceivingMessage(IMessageBufferReader messageBufferReader)
        {

        }

        protected virtual void OnEndReceivingMessage(IMessageBufferReader messageBufferReader)
        {

        }

        protected virtual void OnBeginSendingMessage(IMessageBufferWriter messageBufferWriter)
        {

        }

        protected virtual void OnEndSendingMessage(IMessageBufferWriter messageBufferWriter)
        {

        }


        public void HandleAnyRequest()
        {
            HandleAnyRequestAsync(CancellationToken.None, true).Wait();
        }

        public async Task HandleAnyRequestAsync(CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            OnBeginReceivingMessage(messageBufferReader);

            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != Address)
                throw new InvalidOperationException();

            var functionCode = (ModbusFunctionCode)await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            switch (functionCode)
            {
                case ModbusFunctionCode.ReadDiscreteInputs:
                    {
                        var request = sync ? HandleReadRequest(messageBufferReader) : await HandleReadRequestAsync(messageBufferReader, cancellationToken);
                        await HandleReadDiscreteInputsResponseAsync(request.Offset, request.Count, cancellationToken, sync);
                    }
                    return;

                case ModbusFunctionCode.ReadCoils:
                    {
                        var request = sync ? HandleReadRequest(messageBufferReader) : await HandleReadRequestAsync(messageBufferReader, cancellationToken);
                        await HandleReadCoilsResponseAsync(request.Offset, request.Count, cancellationToken, sync);
                    }
                    return;

                case ModbusFunctionCode.WriteSingleCoil:
                    {
                        var offset = sync ? HandleWriteSingleCoilRequest(messageBufferReader) : await HandleWriteSingleCoilRequestAsync(messageBufferReader, cancellationToken);
                        await HandleWriteSingleCoilResponseAsync(offset, cancellationToken, sync);
                    }
                    return;

                case ModbusFunctionCode.WriteMultipleCoils:
                    {
                        var request = sync ? HandleWriteMultipleCoilsRequest(messageBufferReader) : await HandleWriteMultipleCoilsRequestAsync(cancellationToken);
                        await HandleWriteMultipleCoilsResponseAsync(request.Offset, request.Count, cancellationToken, sync);
                    }
                    return;

                case ModbusFunctionCode.ReadInputRegisters:
                    {
                        var request = HandleReadRequest(messageBufferReader);
                        await HandleReadInputRegistersResponseAsync(request.Offset, request.Count, cancellationToken, sync);
                    }
                    return;

                case ModbusFunctionCode.ReadHoldingRegisters:
                    {
                        var request = sync ? HandleReadRequest(messageBufferReader) : await HandleReadRequestAsync(messageBufferReader, cancellationToken);
                        await HandleReadHoldingRegistersResponseAsync(request.Offset, request.Count, cancellationToken, sync);
                    }
                    return;

                case ModbusFunctionCode.WriteSingleRegister:
                    {
                        var offset = HandleWriteSingleRegisterRequest(messageBufferReader);
                        await HandleWriteSingleRegisterResponseAsync(offset, cancellationToken, sync);
                    }
                    return;

                case ModbusFunctionCode.WriteMultipleRegisters:
                    {
                        var request = sync ? HandleWriteMultipleRegistersRequest(messageBufferReader) : await HandleWriteMultipleRegistersRequestAsync(messageBufferReader, cancellationToken);
                        await HandleWriteMultipleRegistersResponseAsync(request.Offset, request.Count, cancellationToken, sync);
                    }
                    return;

                case ModbusFunctionCode.ReadWriteMultipleRegisters:
                case ModbusFunctionCode.MaskWriteRegister:
                case ModbusFunctionCode.ReadFiFoQueue:
                case ModbusFunctionCode.ReadFileRecord:
                case ModbusFunctionCode.WriteFileRecord:
                case ModbusFunctionCode.ReadExceptionStatus:
                case ModbusFunctionCode.Diagnostic:
                case ModbusFunctionCode.GetComEventCounter:
                case ModbusFunctionCode.GetComEventLog:
                case ModbusFunctionCode.ReportServerID:
                case ModbusFunctionCode.ReadDeviceIdentification:
                    throw new NotImplementedException($"Function '{functionCode}' is not implemented");
            }
        }

        internal void HandleReadCoilsRequest(out int offset, out int count)
        {
            var request = HandleReadCoilsRequest();
            offset = request.Offset;
            count = request.Count;
        }

        internal Request HandleReadCoilsRequest()
            => HandleReadRequest(ModbusFunctionCode.ReadCoils);

        internal void HandleReadCoilsResponse(int zeroBasedOffset, int count)
            => HandleBitArrayResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils, CancellationToken.None, true).Wait();

        internal Task HandleReadCoilsResponseAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken, bool sync = false)
            => HandleBitArrayResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadCoils, cancellationToken, sync);

        internal void HandleReadHoldingRegistersRequest(out int offset, out int count)
        {
            var request = HandleReadHoldingRegistersRequest();
            offset = request.Offset;
            count = request.Count;
        }

        internal Request HandleReadHoldingRegistersRequest()
            => HandleReadRequest(ModbusFunctionCode.ReadHoldingRegisters);

        internal void HandleReadHoldingRegistersResponse(int zeroBasedOffset, int count)
            => HandleRegisterArrayResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters, CancellationToken.None, true).Wait();

        internal Task HandleReadHoldingRegistersResponseAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken, bool sync)
            => HandleRegisterArrayResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadHoldingRegisters, cancellationToken, sync);

        internal void HandleReadDiscreteInputsRequest(out int offset, out int count)
        {
            var request = HandleReadDiscreteInputsRequest();
            offset = request.Offset;
            count = request.Count;
        }

        internal Request HandleReadDiscreteInputsRequest()
            => HandleReadRequest(ModbusFunctionCode.ReadDiscreteInputs);

        internal void HandleReadDiscreteInputsResponse(int zeroBasedOffset, int count)
            => HandleBitArrayResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs, CancellationToken.None, true).Wait();

        internal Task HandleReadDiscreteInputsResponseAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken, bool sync)
            => HandleBitArrayResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadDiscreteInputs, cancellationToken, sync);

        internal void HandleReadInputRegistersRequest(out int offset, out int count)
        {
            var request = HandleReadInputRegistersRequest();
            offset = request.Offset;
            count = request.Count;
        }

        internal Request HandleReadInputRegistersRequest()
            => HandleReadRequest(ModbusFunctionCode.ReadInputRegisters);

        internal void HandleReadInputRegistersResponse(int zeroBasedOffset, int count)
            => HandleRegisterArrayResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters, CancellationToken.None, true).Wait();

        internal Task HandleReadInputRegistersResponseAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken, bool sync)
            => HandleRegisterArrayResponseAsync(zeroBasedOffset, count, ModbusFunctionCode.ReadInputRegisters, cancellationToken, sync);

        internal int HandleWriteSingleCoilRequest()
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            OnBeginReceivingMessage(messageBufferReader);

            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushByteFromStream() != (byte)ModbusFunctionCode.WriteSingleCoil)
                throw new InvalidOperationException();

            return HandleWriteSingleCoilRequest(messageBufferReader);
        }

        internal async Task<int> HandleWriteSingleCoilRequestAsync(CancellationToken cancellation)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            OnBeginReceivingMessage(messageBufferReader);

            if (await messageBufferReader.PushByteFromStreamAsync(cancellation) != Address)
                throw new InvalidOperationException();

            if (await messageBufferReader.PushByteFromStreamAsync(cancellation) != (byte)ModbusFunctionCode.WriteSingleCoil)
                throw new InvalidOperationException();

            return await HandleWriteSingleCoilRequestAsync(messageBufferReader, cancellation);
        }

        internal int HandleWriteSingleCoilRequest(IMessageBufferReader messageBufferReader)
        {
            var zeroBasedOffset = (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));

            var value = messageBufferReader.PushByteFromStream() == 0xFF;

            if (messageBufferReader.PushByteFromStream() != 0x00)
                throw new InvalidOperationException();

            OnEndReceivingMessage(messageBufferReader);

            MemoryMap.OutputCoils[zeroBasedOffset] = value;

            return zeroBasedOffset;
        }

        internal async Task<int> HandleWriteSingleCoilRequestAsync(IMessageBufferReader messageBufferReader, CancellationToken cancellation)
        {
            var zeroBasedOffset = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellation) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellation) << 0));

            var value = await messageBufferReader.PushByteFromStreamAsync(cancellation) == 0xFF;

            if (await messageBufferReader.PushByteFromStreamAsync(cancellation) != 0x00)
                throw new InvalidOperationException();

            OnEndReceivingMessage(messageBufferReader);

            MemoryMap.OutputCoils[zeroBasedOffset] = value;

            return zeroBasedOffset;
        }

        internal void HandleWriteSingleCoilResponse(int zeroBasedOffset)
            => HandleWriteSingleCoilResponseAsync(zeroBasedOffset, CancellationToken.None, true).Wait();

        internal async Task HandleWriteSingleCoilResponseAsync(int zeroBasedOffset, CancellationToken cancellationToken, bool sync = false)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteSingleCoil);

            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)(MemoryMap.OutputCoils[zeroBasedOffset] ? 0xFF : 0x00));
            messageBufferWriter.Push(0);

            OnEndSendingMessage(messageBufferWriter);

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
        }

        internal int HandleWriteSingleRegisterRequest()
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            OnBeginReceivingMessage(messageBufferReader);

            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushByteFromStream() != (byte)ModbusFunctionCode.WriteSingleRegister)
                throw new InvalidOperationException();

            return HandleWriteSingleRegisterRequest(messageBufferReader);
        }

        private int HandleWriteSingleRegisterRequest(IMessageBufferReader messageBufferReader)
        {
            var zeroBasedOffset = (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));
            var value = (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));

            OnEndReceivingMessage(messageBufferReader);

            MemoryMap.OutputRegisters[zeroBasedOffset] = value;

            return zeroBasedOffset;
        }

        internal async Task<int> HandleWriteSingleRegisterRequest(CancellationToken cancellationToken)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            OnBeginReceivingMessage(messageBufferReader);

            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != Address)
                throw new InvalidOperationException();

            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != (byte)ModbusFunctionCode.WriteSingleRegister)
                throw new InvalidOperationException();

            return await HandleWriteSingleRegisterRequest(messageBufferReader, cancellationToken);
        }

        private async Task<int> HandleWriteSingleRegisterRequest(IMessageBufferReader messageBufferReader, CancellationToken cancellationToken)
        {
            var zeroBasedOffset = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));
            var value = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));

            OnEndReceivingMessage(messageBufferReader);

            MemoryMap.OutputRegisters[zeroBasedOffset] = value;

            return zeroBasedOffset;
        }

        internal void HandleWriteSingleRegisterResponse(int zeroBasedOffset)
            => HandleWriteSingleRegisterResponseAsync(zeroBasedOffset, CancellationToken.None, true).Wait();

        internal async Task HandleWriteSingleRegisterResponseAsync(int zeroBasedOffset, CancellationToken cancellationToken, bool sync = false)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteSingleRegister);

            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((MemoryMap.OutputRegisters[zeroBasedOffset] >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((MemoryMap.OutputRegisters[zeroBasedOffset] >> 0) & 0xFF));

            OnEndSendingMessage(messageBufferWriter);

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
        }

        internal void HandleWriteMultipleCoilsRequest(out int offset, out int count)
        {
            var request = HandleWriteMultipleCoilsRequest();
            offset = request.Offset;
            count = request.Count;
        }

        internal Request HandleWriteMultipleCoilsRequest()
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            OnBeginReceivingMessage(messageBufferReader);

            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushByteFromStream() != (byte)ModbusFunctionCode.WriteMultipleCoils)
                throw new InvalidOperationException();

            return HandleWriteMultipleCoilsRequest(messageBufferReader);
        }

        private Request HandleWriteMultipleCoilsRequest(IMessageBufferReader messageBufferReader)
        {
            var zeroBasedOffset = (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));

            var countOfValues = (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));

            var byteCount = messageBufferReader.PushByteFromStream();

            messageBufferReader.PushFromStream(byteCount);

            OnEndReceivingMessage(messageBufferReader);

            //update the memory map
            MemoryMap.OutputCoils.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, countOfValues);

            return new Request(zeroBasedOffset, countOfValues);
        }

        internal async Task<Request> HandleWriteMultipleCoilsRequestAsync(CancellationToken cancellationToken)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            OnBeginReceivingMessage(messageBufferReader);

            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != Address)
                throw new InvalidOperationException();

            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != (byte)ModbusFunctionCode.WriteMultipleCoils)
                throw new InvalidOperationException();

            return await HandleWriteMultipleCoilsRequestAsync(messageBufferReader, cancellationToken);
        }

        private async Task<Request> HandleWriteMultipleCoilsRequestAsync(IMessageBufferReader messageBufferReader, CancellationToken cancellationToken)
        {
            var zeroBasedOffset = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));

            var countOfValues = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));

            var byteCount = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            await messageBufferReader.PushFromStreamAsync(byteCount, cancellationToken);

            OnEndReceivingMessage(messageBufferReader);

            //update the memory map
            MemoryMap.OutputCoils.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, countOfValues);

            return new Request(zeroBasedOffset, countOfValues);
        }

        internal void HandleWriteMultipleCoilsResponse(int zeroBasedOffset, int count)
            => HandleWriteMultipleCoilsResponseAsync(zeroBasedOffset, count, CancellationToken.None, true).Wait();

        internal async Task HandleWriteMultipleCoilsResponseAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken, bool sync = false)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteMultipleCoils);

            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            OnEndSendingMessage(messageBufferWriter);

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
        }

        internal void HandleWriteMultipleRegistersRequest(out int offset, out int count)
        {
            var request = HandleWriteMultipleRegistersRequest();
            offset = request.Offset;
            count = request.Count;
        }

        internal Request HandleWriteMultipleRegistersRequest()
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            OnBeginReceivingMessage(messageBufferReader);

            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushByteFromStream() != (byte)ModbusFunctionCode.WriteMultipleRegisters)
                throw new InvalidOperationException();

            return HandleWriteMultipleRegistersRequest(messageBufferReader);
        }

        internal Request HandleWriteMultipleRegistersRequest(IMessageBufferReader messageBufferReader)
        {
            var zeroBasedOffset = (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));

            var countOfValues = (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));

            var byteCount = messageBufferReader.PushByteFromStream();

            messageBufferReader.PushFromStream(byteCount);

            OnEndReceivingMessage(messageBufferReader);

            //update the memory map
            MemoryMap.OutputRegisters.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, countOfValues);

            return new Request(zeroBasedOffset, countOfValues);
        }

        internal async Task<Request> HandleWriteMultipleRegistersRequestAsync(CancellationToken cancellationToken)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            OnBeginReceivingMessage(messageBufferReader);

            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != Address)
                throw new InvalidOperationException();

            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != (byte)ModbusFunctionCode.WriteMultipleRegisters)
                throw new InvalidOperationException();

            return await HandleWriteMultipleRegistersRequestAsync(messageBufferReader, cancellationToken);
        }

        internal async Task<Request> HandleWriteMultipleRegistersRequestAsync(IMessageBufferReader messageBufferReader, CancellationToken cancellationToken)
        {
            var zeroBasedOffset = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));

            var countOfValues = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));

            var byteCount = messageBufferReader.PushByteFromStream();

            await messageBufferReader.PushFromStreamAsync(byteCount, cancellationToken);

            OnEndReceivingMessage(messageBufferReader);

            //update the memory map
            MemoryMap.OutputRegisters.CopyFrom(
                        new MessageBufferSpan(_messageBuffer, (ushort)(_messageBuffer.Length - byteCount - 2), byteCount), zeroBasedOffset, countOfValues);

            return new Request(zeroBasedOffset, countOfValues);
        }

        internal void HandleWriteMultipleRegistersResponse(int zeroBasedOffset, int count)
            => HandleWriteMultipleRegistersResponseAsync(zeroBasedOffset, count, CancellationToken.None, true).Wait();

        internal async Task HandleWriteMultipleRegistersResponseAsync(int zeroBasedOffset, int count, CancellationToken cancellationToken, bool sync = false)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteMultipleRegisters);

            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            OnEndSendingMessage(messageBufferWriter);

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
        }

        private void HandleBitArrayResponse(int zeroBasedOffset, int count, ModbusFunctionCode functionCode)
            => HandleBitArrayResponseAsync(zeroBasedOffset, count, functionCode, CancellationToken.None, true).Wait();

        private async Task HandleBitArrayResponseAsync(int zeroBasedOffset, int count, ModbusFunctionCode functionCode, CancellationToken cancellationToken, bool sync = false)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

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

            OnEndSendingMessage(messageBufferWriter);

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
        }

        private void HandleRegisterArrayResponse(int zeroBasedOffset, int count, ModbusFunctionCode functionCode)
            => HandleRegisterArrayResponseAsync(zeroBasedOffset, count, functionCode, CancellationToken.None, true).Wait();
        //{
        //    //write the response
        //    using var messageBufferWriter = _messageBuffer.BeginWrite();

        //    OnBeginSendingMessage(messageBufferWriter);

        //    messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
        //    messageBufferWriter.Push((byte)functionCode);

        //    if (functionCode == ModbusFunctionCode.ReadHoldingRegisters)
        //    {
        //        MemoryMap.OutputRegisters.CopyTo(messageBufferWriter, zeroBasedOffset, count);
        //    }
        //    else
        //    {
        //        MemoryMap.InputRegisters.CopyTo(messageBufferWriter, zeroBasedOffset, count);
        //    }

        //    OnEndSendingMessage(messageBufferWriter);

        //    _messageBuffer.WriteToStream(Stream);
        //}

        private async Task HandleRegisterArrayResponseAsync(int zeroBasedOffset, int count, ModbusFunctionCode functionCode, CancellationToken cancellationToken, bool sync = false)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

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

            OnEndSendingMessage(messageBufferWriter);

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
        }

        private Request HandleReadRequest(ModbusFunctionCode functionCode)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (messageBufferReader.PushByteFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushByteFromStream() != (byte)functionCode)
                throw new InvalidOperationException();

            return HandleReadRequest(messageBufferReader);
        }

        private Request HandleReadRequest(IMessageBufferReader messageBufferReader)
        {
            var zeroBasedOffset = (ushort)((messageBufferReader.PushByteFromStream() << 8) +
                (messageBufferReader.PushByteFromStream() << 0));
            var count = (ushort)((messageBufferReader.PushByteFromStream() << 8) +
                (messageBufferReader.PushByteFromStream() << 0));

            OnEndReceivingMessage(messageBufferReader);

            return new Request(zeroBasedOffset, count);
        }

        private async Task<Request> HandleReadRequestAsync(ModbusFunctionCode functionCode, CancellationToken cancellationToken)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != Address)
                throw new InvalidOperationException();

            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != (byte)functionCode)
                throw new InvalidOperationException();

            return HandleReadRequest(messageBufferReader);
        }

        private async Task<Request> HandleReadRequestAsync(IMessageBufferReader messageBufferReader, CancellationToken cancellationToken)
        {
            var zeroBasedOffset = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));
            var count = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));

            OnEndReceivingMessage(messageBufferReader);

            return new Request(zeroBasedOffset, count);
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

        public async Task WriteSingleCoilAsync(int zeroBasedOffset, bool value, CancellationToken cancellationToken)
        {
            await SendWriteSingleCoilRequestAsync(zeroBasedOffset, value, cancellationToken);

            await ReceiveWriteSingleCoilResponseAsync(zeroBasedOffset, cancellationToken);
        }

        public void WriteSingleRegister(int zeroBasedOffset, int value)
        {
            SendWriteSingleRegisterRequest(zeroBasedOffset, value);

            ReceiveWriteSingleRegisterResponse(zeroBasedOffset);
        }

        public async Task WriteSingleRegisterAsync(int zeroBasedOffset, int value, CancellationToken cancellationToken)
        {
            await SendWriteSingleRegisterRequestAsync(zeroBasedOffset, value, cancellationToken);

            await ReceiveWriteSingleRegisterResponseAsync(zeroBasedOffset, cancellationToken);
        }

        public void WriteMultipleCoils(int zeroBasedOffset, bool[] values)
        {
            Validate.Between(nameof(values) + ".Length", values.Length, 0, 0x07B0);

            SendWriteMultipleCoilsRequest(zeroBasedOffset, values);

            ReceiveWriteMultipleCoilsResponse(zeroBasedOffset);//, out var countOfValuesReturned);

            //if (countOfValuesReturned != values.Length)
            //    throw new InvalidOperationException();
        }

        public async Task WriteMultipleCoilsAsync(int zeroBasedOffset, bool[] values, CancellationToken cancellationToken)
        {
            Validate.Between(nameof(values) + ".Length", values.Length, 0, 0x07B0);

            await SendWriteMultipleCoilsRequestAsync(zeroBasedOffset, values, cancellationToken);

            await ReceiveWriteMultipleCoilsResponseAsync(zeroBasedOffset, cancellationToken);//, out var countOfValuesReturned);

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

        public async Task WriteMultipleRegistersAsync(int zeroBasedOffset, int[] values, CancellationToken cancellationToken)
        {
            Validate.Between(nameof(values) + ".Length", values.Length, 0, 0x07B0);

            await SendWriteMultipleRegistersRequestAsync(zeroBasedOffset, values, cancellationToken);

            await ReceiveWriteMultipleRegistersResponseAsync(zeroBasedOffset, cancellationToken);//, out var countOfValuesReturned);

            //if (countOfValuesReturned != values.Length)
            //    throw new InvalidOperationException();
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
            => SendWriteSingleCoilRequestAsync(zeroBasedOffset, value, CancellationToken.None, true).Wait();

        internal Task SendWriteSingleCoilRequestAsync(int zeroBasedOffset, bool value, CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

            PushWriteSingleCoilRequest(messageBufferWriter, Address, zeroBasedOffset, value);

            OnEndSendingMessage(messageBufferWriter);

            return _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
        }

        internal void SendWriteSingleRegisterRequest(int zeroBasedOffset, int value)
            => SendWriteSingleRegisterRequestAsync(zeroBasedOffset, value, CancellationToken.None, true).Wait();

        internal Task SendWriteSingleRegisterRequestAsync(int zeroBasedOffset, int value, CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

            PushWriteSingleRegisterRequest(messageBufferWriter, Address, zeroBasedOffset, value);

            OnEndSendingMessage(messageBufferWriter);

            return _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
        }

        internal void SendWriteMultipleCoilsRequest(int zeroBasedOffset, params bool[] values)
            => SendWriteMultipleCoilsRequestAsync(zeroBasedOffset, values, CancellationToken.None, true).Wait();

        internal Task SendWriteMultipleCoilsRequestAsync(int zeroBasedOffset, bool[] values, CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

            PushWriteMultipleCoilsRequest(messageBufferWriter, Address, zeroBasedOffset, values);

            OnEndSendingMessage(messageBufferWriter);

            return _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
        }

        internal void SendWriteMultipleRegistersRequest(int zeroBasedOffset, params int[] values)
            => SendWriteMultipleRegistersRequestAsync(zeroBasedOffset, values, CancellationToken.None, true).Wait();

        internal Task SendWriteMultipleRegistersRequestAsync(int zeroBasedOffset, int[] values, CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

            PushWriteMultipleRegistersRequest(messageBufferWriter, Address, zeroBasedOffset, values);

            OnEndSendingMessage(messageBufferWriter);

            return _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
        }

        private void SendReadRequest(int zeroBasedOffset, int count, ModbusFunctionCode functionCode)
            => SendReadRequestAsync(zeroBasedOffset, count, functionCode, CancellationToken.None, true).Wait();

        private async Task SendReadRequestAsync(int zeroBasedOffset, int count, ModbusFunctionCode functionCode, CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferWriter = _messageBuffer.BeginWrite();

            OnBeginSendingMessage(messageBufferWriter);

            PushReadRequest(messageBufferWriter, Address, zeroBasedOffset, count, functionCode);

            OnEndSendingMessage(messageBufferWriter);

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken, sync);
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
            => ReceiveWriteSingleCoilResponseAsync(zeroBasedOffset, CancellationToken.None, true).Wait();

        internal async Task ReceiveWriteSingleCoilResponseAsync(int zeroBasedOffset, CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (sync)
            {
                ReceiveWriteSingleCoilResponse(messageBufferReader, Address, zeroBasedOffset);
            }
            else
            {
                await ReceiveWriteSingleCoilResponseAsync(messageBufferReader, Address, zeroBasedOffset, cancellationToken);
            }

            OnEndReceivingMessage(messageBufferReader);

            //MemoryMap.OutputCoils[zeroBasedOffset] = value;
        }

        internal void ReceiveWriteMultipleCoilsResponse(int zeroBasedOffset)
            => ReceiveWriteMultipleCoilsResponseAsync(zeroBasedOffset, CancellationToken.None, true).Wait();

        internal async Task ReceiveWriteMultipleCoilsResponseAsync(int zeroBasedOffset, CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (sync)
            {
                ReceiveWriteMultipleCoilsResponse(messageBufferReader, Address, zeroBasedOffset);
            }
            else
            {
                await ReceiveWriteMultipleCoilsResponseAsync(messageBufferReader, Address, zeroBasedOffset, cancellationToken);
            }

            OnEndReceivingMessage(messageBufferReader);
        }

        internal void ReceiveWriteSingleRegisterResponse(int zeroBasedOffset)
            => ReceiveWriteSingleRegisterResponseAsync(zeroBasedOffset, CancellationToken.None, true).Wait();

        internal async Task ReceiveWriteSingleRegisterResponseAsync(int zeroBasedOffset, CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (sync)
            {
                ReceiveWriteSingleRegisterResponse(messageBufferReader, Address, zeroBasedOffset);
            }
            else
            {
                await ReceiveWriteSingleRegisterResponseAsync(messageBufferReader, Address, zeroBasedOffset, cancellationToken);
            }

            //MemoryMap.OutputRegisters[zeroBasedOffset] = value;
        }

        internal void ReceiveWriteMultipleRegistersResponse(int zeroBasedOffset)
            => ReceiveWriteMultipleRegistersResponseAsync(zeroBasedOffset, CancellationToken.None, true).Wait();

        internal async Task ReceiveWriteMultipleRegistersResponseAsync(int zeroBasedOffset, CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (sync)
            {
                ReceiveWriteMultipleRegistersResponse(messageBufferReader, Address, zeroBasedOffset);
            }
            else
            {
                await ReceiveWriteMultipleRegistersResponseAsync(messageBufferReader, Address, zeroBasedOffset, cancellationToken);
            }

            OnEndReceivingMessage(messageBufferReader);
        }

        private void ReceiveReadResponse(int zeroBasedOffset, int count, ModbusFunctionCode expectedFunctionCode)
            => ReceiveReadResponseAsync(zeroBasedOffset, count, expectedFunctionCode, CancellationToken.None).Wait();

        private async Task ReceiveReadResponseAsync(int zeroBasedOffset, int count, ModbusFunctionCode expectedFunctionCode, CancellationToken cancellationToken, bool sync = false)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            var byteCount = sync ?
                PushReceiveReadResponse(messageBufferReader, Address, zeroBasedOffset, count, expectedFunctionCode)
                :
                await PushReceiveReadResponseAsync(messageBufferReader, Address, zeroBasedOffset, count, expectedFunctionCode, cancellationToken);

            OnEndReceivingMessage(messageBufferReader);

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


        public void PushWriteSingleCoilRequest(IMessageBufferWriter messageBufferWriter, byte address, int zeroBasedOffset, bool value)
        {
            messageBufferWriter.Push(address);
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteSingleCoil);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)(value ? 0xFF : 0x00));
            messageBufferWriter.Push(0);
        }

        public void PushWriteSingleRegisterRequest(IMessageBufferWriter messageBufferWriter, byte address, int zeroBasedOffset, int value)
        {
            messageBufferWriter.Push(address);
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteSingleRegister);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((value >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((value >> 0) & 0xFF));
        }

        public void PushWriteMultipleCoilsRequest(IMessageBufferWriter messageBufferWriter, byte address, int zeroBasedOffset, params bool[] values)
        {
            messageBufferWriter.Push(address);
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteMultipleCoils);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));

            int count = values.Length;
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            values.CopyTo(messageBufferWriter);
        }

        public void PushWriteMultipleRegistersRequest(IMessageBufferWriter messageBufferWriter, byte address, int zeroBasedOffset, params int[] values)
        {
            messageBufferWriter.Push(address);
            messageBufferWriter.Push((byte)ModbusFunctionCode.WriteMultipleRegisters);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));

            int count = values.Length;
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));

            values.CopyTo(messageBufferWriter);
        }

        public void PushReadRequest(IMessageBufferWriter messageBufferWriter, byte address, int zeroBasedOffset, int count, ModbusFunctionCode functionCode)
        {
            messageBufferWriter.Push(address);
            messageBufferWriter.Push((byte)functionCode);
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 8) & 0xFF));
            messageBufferWriter.Push((byte)((count >> 0) & 0xFF));
        }

        public byte PushReceiveReadResponse(IMessageBufferReader messageBufferReader, byte address, int zeroBasedOffset, int count, ModbusFunctionCode expectedFunctionCode)
        {
            if (messageBufferReader.PushByteFromStream() != address)
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

            return byteCount;
        }

        public async Task<byte> PushReceiveReadResponseAsync(IMessageBufferReader messageBufferReader, byte address, int zeroBasedOffset, int count, ModbusFunctionCode expectedFunctionCode, CancellationToken cancellationToken)
        {
            if (address != await messageBufferReader.PushByteFromStreamAsync(cancellationToken))
                throw new InvalidOperationException();

            byte functionCode = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            if (functionCode == ((byte)expectedFunctionCode | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)(await messageBufferReader.PushByteFromStreamAsync(cancellationToken));
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)expectedFunctionCode)
                throw new InvalidOperationException();

            var byteCount = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            await messageBufferReader.PushFromStreamAsync(byteCount, cancellationToken);

            return byteCount;
        }

        public ushort ReceiveWriteMultipleRegistersResponse(IMessageBufferReader messageBufferReader, byte address, int zeroBasedOffset)
        {
            if (messageBufferReader.PushByteFromStream() != address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushByteFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.WriteMultipleRegisters | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushByteFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteMultipleRegisters)
                throw new InvalidOperationException();

            if ((ushort)((messageBufferReader.PushByteFromStream() << 8) +
                (messageBufferReader.PushByteFromStream() << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var count = (ushort)((messageBufferReader.PushByteFromStream() << 8) +
                (messageBufferReader.PushByteFromStream() << 0));

            return count;
        }

        public async Task<ushort> ReceiveWriteMultipleRegistersResponseAsync(IMessageBufferReader messageBufferReader, byte address, int zeroBasedOffset, CancellationToken cancellationToken)
        {
            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != address)
                throw new InvalidOperationException();

            byte functionCode = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            if (functionCode == ((byte)ModbusFunctionCode.WriteMultipleRegisters | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)await messageBufferReader.PushByteFromStreamAsync(cancellationToken);
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteMultipleRegisters)
                throw new InvalidOperationException();

            if ((ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var count = (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));

            return count;
        }

        public ushort ReceiveWriteSingleRegisterResponse(IMessageBufferReader messageBufferReader, byte address, int zeroBasedOffset)
        {
            if (messageBufferReader.PushByteFromStream() != address)
                throw new InvalidOperationException();

            byte functionCode = messageBufferReader.PushByteFromStream();

            if (functionCode == ((byte)ModbusFunctionCode.WriteSingleRegister | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)messageBufferReader.PushByteFromStream();
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteSingleRegister)
                throw new InvalidOperationException();

            if ((ushort)((messageBufferReader.PushByteFromStream() << 8) +
                (messageBufferReader.PushByteFromStream() << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            return (ushort)((messageBufferReader.PushByteFromStream() << 8) +
                (messageBufferReader.PushByteFromStream() << 0));
        }

        public async Task<ushort> ReceiveWriteSingleRegisterResponseAsync(IMessageBufferReader messageBufferReader, byte address, int zeroBasedOffset, CancellationToken cancellationToken)
        {
            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != address)
                throw new InvalidOperationException();

            byte functionCode = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            if (functionCode == ((byte)ModbusFunctionCode.WriteSingleRegister | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)await messageBufferReader.PushByteFromStreamAsync(cancellationToken);
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteSingleRegister)
                throw new InvalidOperationException();

            if ((ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            return (ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0));

        }

        public bool ReceiveWriteSingleCoilResponse(IMessageBufferReader messageBufferReader, byte address, int zeroBasedOffset)
        {
            if (messageBufferReader.PushByteFromStream() != address)
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

            return value;
        }

        public async Task<bool> ReceiveWriteSingleCoilResponseAsync(IMessageBufferReader messageBufferReader, byte address, int zeroBasedOffset, CancellationToken cancellationToken)
        {
            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != address)
                throw new InvalidOperationException();

            byte functionCode = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            if (functionCode == ((byte)ModbusFunctionCode.WriteSingleCoil | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)await messageBufferReader.PushByteFromStreamAsync(cancellationToken);
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteSingleCoil)
                throw new InvalidOperationException();

            if ((ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            var value = await messageBufferReader.PushByteFromStreamAsync(cancellationToken) == 0xFF;

            await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            return value;
        }

        public ushort ReceiveWriteMultipleCoilsResponse(IMessageBufferReader messageBufferReader, byte address, int zeroBasedOffset)
        {
            if (messageBufferReader.PushByteFromStream() != address)
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

            return (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));
        }

        public async Task<ushort> ReceiveWriteMultipleCoilsResponseAsync(IMessageBufferReader messageBufferReader, byte address, int zeroBasedOffset, CancellationToken cancellationToken)
        {
            if (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) != address)
                throw new InvalidOperationException();

            byte functionCode = await messageBufferReader.PushByteFromStreamAsync(cancellationToken);

            if (functionCode == ((byte)ModbusFunctionCode.WriteMultipleCoils | 0x80))
            {
                var exceptionCode = (ModbusExceptionCode)await messageBufferReader.PushByteFromStreamAsync(cancellationToken);
                throw new ModbusException(exceptionCode);
            }

            if (functionCode != (byte)ModbusFunctionCode.WriteMultipleCoils)
                throw new InvalidOperationException();

            if ((ushort)((await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 8) +
                (await messageBufferReader.PushByteFromStreamAsync(cancellationToken) << 0)) != zeroBasedOffset)
                throw new InvalidOperationException();

            return (ushort)((messageBufferReader.PushByteFromStream() << 8) + (messageBufferReader.PushByteFromStream() << 0));
        }
    }
}
