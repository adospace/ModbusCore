using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
