using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusCore
{
    public partial class ModbusClient
    {
        public bool WriteSingleCoil(ModbusDevice device, int zeroBasedOffset, bool value)
            => Write<bool>(device, ModbusFunctionCode.WriteSingleCoil, zeroBasedOffset,
                writer =>
                {
                    writer.Push((byte)(value ? 0xFF : 0x00));
                    writer.Push(0);
                },
                reader =>
                {
                    var value = reader.PushByteFromStream() == 0xFF;
                    reader.PushByteFromStream();

                    return value;
                });

        public int WriteSingleRegister(ModbusDevice device, int zeroBasedOffset, int value)
            => Write<int>(device, ModbusFunctionCode.WriteSingleRegister, zeroBasedOffset,
                writer => 
                {
                    writer.Push((byte)((value >> 8) & 0xFF));
                    writer.Push((byte)((value >> 0) & 0xFF));
                },
                reader => 
                {
                    return (ushort)((reader.PushByteFromStream() << 8) +
                        (reader.PushByteFromStream() << 0));
                });

        public void WriteMultipleCoils(ModbusDevice device, int zeroBasedOffset, bool[] values)
            => Write<int>(device, ModbusFunctionCode.WriteMultipleCoils, zeroBasedOffset,
                writer =>
                {
                    int count = values.Length;
                    writer.Push((byte)((count >> 8) & 0xFF));
                    writer.Push((byte)((count >> 0) & 0xFF));

                    values.CopyTo(writer);
                },
                reader =>
                {
                    return (ushort)((reader.PushByteFromStream() << 8) +
                        (reader.PushByteFromStream() << 0));
                });

        public void WriteMultipleRegisters(ModbusDevice device, int zeroBasedOffset, int[] values)
            => Write<int>(device, ModbusFunctionCode.WriteMultipleRegisters, zeroBasedOffset,
                writer =>
                {
                    int count = values.Length;
                    writer.Push((byte)((count >> 8) & 0xFF));
                    writer.Push((byte)((count >> 0) & 0xFF));

                    values.CopyTo(writer);
                },
                reader =>
                {
                    return (ushort)((reader.PushByteFromStream() << 8) +
                        (reader.PushByteFromStream() << 0));
                });

        private T Write<T>(ModbusDevice device, ModbusFunctionCode functionCode,int zeroBasedOffset, Action<IMessageBufferWriter> writeAction, Func<IMessageBufferReader, T> readValueFunc)
        {
            var requestMessage = new ModbusMessage()
            {
                TransactionIdentifier = GetTransactionIdentifier()
            };

            ModbusTransport.SendMessage(requestMessage,
                (writer) =>
                {
                    writer.Push(device.Address);
                    writer.Push((byte)functionCode);
                    writer.Push((byte)((zeroBasedOffset >> 8) & 0xFF));
                    writer.Push((byte)((zeroBasedOffset >> 0) & 0xFF));
                    writeAction(writer);
                });

            T returnedValue = default;

            ModbusTransport.ReceiveMessage<ModbusResponseMessage>(
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

                    if ((ushort)((reader.PushByteFromStream() << 8) + (reader.PushByteFromStream() << 0)) != zeroBasedOffset)
                        throw new InvalidOperationException();

                    returnedValue = readValueFunc(reader);

                    return new ModbusResponseMessage(requestMessage);
                });

            return returnedValue!;

        }
    }
}
