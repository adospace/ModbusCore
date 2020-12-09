using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public partial class ModbusClient
    {
        public void ReadCoils(ModbusDevice device, int zeroBasedOffset, int count)
            => Read(device, ModbusFunctionCode.ReadCoils, zeroBasedOffset, count, receivedBuffer => device.MemoryMap.OutputCoils.CopyFrom(receivedBuffer, zeroBasedOffset, count));
        public void ReadDiscreteInputs(ModbusDevice device, int zeroBasedOffset, int count)
            => Read(device, ModbusFunctionCode.ReadDiscreteInputs, zeroBasedOffset, count, receivedBuffer => device.MemoryMap.InputCoils.CopyFrom(receivedBuffer, zeroBasedOffset, count));
        public void ReadHoldingRegisters(ModbusDevice device, int zeroBasedOffset, int count)
            => Read(device, ModbusFunctionCode.ReadHoldingRegisters, zeroBasedOffset, count, receivedBuffer => device.MemoryMap.OutputRegisters.CopyFrom(receivedBuffer, zeroBasedOffset, count));
        public void ReadInputRegisters(ModbusDevice device, int zeroBasedOffset, int count)
            => Read(device, ModbusFunctionCode.ReadInputRegisters, zeroBasedOffset, count, receivedBuffer => device.MemoryMap.InputRegisters.CopyFrom(receivedBuffer, zeroBasedOffset, count));
        
        private void Read(ModbusDevice device, ModbusFunctionCode functionCode, int zeroBasedOffset, int count, Action<MessageBufferSpan> actionWithReturnedBuffer)
        {
            var requestContext = new ModbusTransportContext()
            {
                TransactionIdentifier = GetTransactionIdentifier()
            };

            ModbusTransport.SendMessage(requestContext,
                (writer) =>
                {
                    writer.Push(device.Address);
                    writer.Push((byte)functionCode);
                    writer.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
                    writer.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
                    writer.Push((byte)((count >> 8) & 0xFF));
                    writer.Push((byte)((count >> 0) & 0xFF));
                });

            MessageBufferSpan? receivedBuffer = null;

            var responseContext = ModbusTransport.ReceiveMessage(
                (reader) =>
                {
                    if (reader.PushByteFromStream() != device.Address)
                        throw new InvalidOperationException();

                    byte receivedFunctionCode = reader.PushByteFromStream();

                    if (receivedFunctionCode == ((byte)functionCode | 0x80))
                    {
                        var exceptionCode = (ModbusExceptionCode)reader.PushByteFromStream();
                        throw new ModbusException(exceptionCode);
                    }

                    if (receivedFunctionCode != (byte)functionCode)
                        throw new InvalidOperationException();

                    var byteCount = reader.PushByteFromStream();

                    reader.PushFromStream(byteCount);

                    receivedBuffer = new MessageBufferSpan(reader.Buffer, (ushort)(reader.Buffer.Length - byteCount), byteCount);
                });

            if (responseContext.TransactionIdentifier != requestContext.TransactionIdentifier)
            {
                throw new InvalidOperationException();
            }

            actionWithReturnedBuffer(receivedBuffer!);
        }


        public Task ReadCoilsAsync(ModbusDevice device, int zeroBasedOffset, int count, CancellationToken cancellationToken)
            => ReadAsync(device, ModbusFunctionCode.ReadCoils, zeroBasedOffset, count, receivedBuffer => device.MemoryMap.OutputCoils.CopyFrom(receivedBuffer, zeroBasedOffset, count), cancellationToken);
        public Task ReadDiscreteInputsAsync(ModbusDevice device, int zeroBasedOffset, int count, CancellationToken cancellationToken)
            => ReadAsync(device, ModbusFunctionCode.ReadDiscreteInputs, zeroBasedOffset, count, receivedBuffer => device.MemoryMap.InputCoils.CopyFrom(receivedBuffer, zeroBasedOffset, count), cancellationToken);
        public Task ReadHoldingRegistersAsync(ModbusDevice device, int zeroBasedOffset, int count, CancellationToken cancellationToken)
            => ReadAsync(device, ModbusFunctionCode.ReadHoldingRegisters, zeroBasedOffset, count, receivedBuffer => device.MemoryMap.OutputRegisters.CopyFrom(receivedBuffer, zeroBasedOffset, count), cancellationToken);
        public Task ReadInputRegistersAsync(ModbusDevice device, int zeroBasedOffset, int count, CancellationToken cancellationToken)
            => ReadAsync(device, ModbusFunctionCode.ReadInputRegisters, zeroBasedOffset, count, receivedBuffer => device.MemoryMap.InputRegisters.CopyFrom(receivedBuffer, zeroBasedOffset, count), cancellationToken);


        private async Task ReadAsync(ModbusDevice device, ModbusFunctionCode functionCode, int zeroBasedOffset, int count, Action<MessageBufferSpan> actionWithReturnedBuffer, CancellationToken cancellationToken)
        {
            var requestContext = new ModbusTransportContext()
            {
                TransactionIdentifier = GetTransactionIdentifier()
            };

            await ModbusTransport.SendMessageAsync(requestContext,
                (writer) =>
                {
                    writer.Push(device.Address);
                    writer.Push((byte)functionCode);
                    writer.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
                    writer.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
                    writer.Push((byte)((count >> 8) & 0xFF));
                    writer.Push((byte)((count >> 0) & 0xFF));
                }, cancellationToken);

            MessageBufferSpan? receivedBuffer = null;

            var responseContext = await ModbusTransport.ReceiveMessageAsync(
                async (reader) =>
                {
                    if (await reader.PushByteFromStreamAsync(cancellationToken) != device.Address)
                        throw new InvalidOperationException();

                    byte receivedFunctionCode = await reader.PushByteFromStreamAsync(cancellationToken);

                    if (receivedFunctionCode == ((byte)functionCode | 0x80))
                    {
                        var exceptionCode = (ModbusExceptionCode)await reader.PushByteFromStreamAsync(cancellationToken);
                        throw new ModbusException(exceptionCode);
                    }

                    if (receivedFunctionCode != (byte)functionCode)
                        throw new InvalidOperationException();

                    var byteCount = await reader.PushByteFromStreamAsync(cancellationToken);

                    await reader.PushFromStreamAsync(byteCount, cancellationToken);

                    receivedBuffer = new MessageBufferSpan(reader.Buffer, (ushort)(reader.Buffer.Length - byteCount), byteCount);

                }, cancellationToken);

            if (responseContext.TransactionIdentifier != requestContext.TransactionIdentifier)
            {
                throw new InvalidOperationException();
            }

            actionWithReturnedBuffer(receivedBuffer!);
        }
    }
}
