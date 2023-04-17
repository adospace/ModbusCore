using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public partial class ModbusClient
    {
        public bool WriteSingleCoil(ModbusDevice device, int zeroBasedOffset, bool value)
            => Write(device, ModbusFunctionCode.WriteSingleCoil, zeroBasedOffset,
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

        public Task<bool> WriteSingleCoilAsync(ModbusDevice device, int zeroBasedOffset, bool value, CancellationToken cancellationToken)
            => WriteAsync(device, ModbusFunctionCode.WriteSingleCoil, zeroBasedOffset,
                writer =>
                {
                    writer.Push((byte)(value ? 0xFF : 0x00));
                    writer.Push(0);
                },
                async reader =>
                {
                    var value = await reader.PushByteFromStreamAsync(cancellationToken) == 0xFF;
                    await reader.PushByteFromStreamAsync(cancellationToken);

                    return value;
                }, cancellationToken);

        public int WriteSingleRegister(ModbusDevice device, int zeroBasedOffset, int value)
            => Write(device, ModbusFunctionCode.WriteSingleRegister, zeroBasedOffset,
                writer => 
                {
                    writer.Push((byte)((value >> 8) & 0xFF));
                    writer.Push((byte)((value >> 0) & 0xFF));
                },
                reader => 
                {
                    return reader.PushShortFromStream();
                });

        public Task<int> WriteSingleRegisterAsync(ModbusDevice device, int zeroBasedOffset, int value, CancellationToken cancellationToken)
            => WriteAsync(device, ModbusFunctionCode.WriteSingleRegister, zeroBasedOffset,
                writer =>
                {
                    writer.Push((byte)((value >> 8) & 0xFF));
                    writer.Push((byte)((value >> 0) & 0xFF));
                },
                async reader =>
                {
                    var value = (int)await reader.PushShortFromStreamAsync(cancellationToken);

                    return value;
                }, cancellationToken);

        public void WriteMultipleCoils(ModbusDevice device, int zeroBasedOffset, bool[] values)
            => Write(device, ModbusFunctionCode.WriteMultipleCoils, zeroBasedOffset,
                writer =>
                {
                    int count = values.Length;
                    writer.Push((byte)((count >> 8) & 0xFF));
                    writer.Push((byte)((count >> 0) & 0xFF));

                    values.CopyTo(writer);
                },
                reader =>
                {
                    return reader.PushShortFromStream();
                });

        public Task WriteMultipleCoilsAsync(ModbusDevice device, int zeroBasedOffset, bool[] values, CancellationToken cancellationToken)
            => WriteAsync(device, ModbusFunctionCode.WriteMultipleCoils, zeroBasedOffset,
                writer =>
                {
                    int count = values.Length;
                    writer.Push((byte)((count >> 8) & 0xFF));
                    writer.Push((byte)((count >> 0) & 0xFF));

                    values.CopyTo(writer);
                },
                async reader =>
                {
                    return await reader.PushShortFromStreamAsync(cancellationToken);
                }, cancellationToken);

        public void WriteMultipleRegisters(ModbusDevice device, int zeroBasedOffset, int[] values)
            => Write(device, ModbusFunctionCode.WriteMultipleRegisters, zeroBasedOffset,
                writer =>
                {
                    int count = values.Length;
                    writer.Push((byte)((count >> 8) & 0xFF));
                    writer.Push((byte)((count >> 0) & 0xFF));

                    values.CopyTo(writer);
                },
                reader =>
                {
                    return reader.PushShortFromStream();
                });

        public Task WriteMultipleRegistersAsync(ModbusDevice device, int zeroBasedOffset, int[] values, CancellationToken cancellationToken)
            => WriteAsync(device, ModbusFunctionCode.WriteMultipleRegisters, zeroBasedOffset,
                writer =>
                {
                    int count = values.Length;
                    writer.Push((byte)((count >> 8) & 0xFF));
                    writer.Push((byte)((count >> 0) & 0xFF));

                    values.CopyTo(writer);
                },
                async (reader) =>
                {
                    return await reader.PushShortFromStreamAsync(cancellationToken);
                }, cancellationToken);

        private T Write<T>(ModbusDevice device, ModbusFunctionCode functionCode, int zeroBasedOffset, Action<IMessageBufferWriter> writeAction, Func<IMessageBufferReader, T> readValueFunc) where T : struct
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
                    writeAction(writer);
                });

            T returnedValue = default;

            var responseContext = ModbusTransport.ReceiveMessage(
                (reader) =>
                {
                    if (reader.PushByteFromStream() != device.Address)
                        throw new InvalidOperationException($"Unable to read the address ({device.Address}) from the reply");

                    byte receivedFunctionCode = reader.PushByteFromStream();

                    if (receivedFunctionCode == ((byte)functionCode | 0x80))
                    {
                        var exceptionCode = (ModbusExceptionCode)reader.PushByteFromStream();
                        throw new ModbusException(exceptionCode);
                    }

                    if (receivedFunctionCode != (byte)functionCode)
                        throw new InvalidOperationException($"Expected function {functionCode} from reply: received {receivedFunctionCode} instead");

                    var receivedOffset = reader.PushShortFromStream();
                    if (receivedOffset != zeroBasedOffset)
                        throw new InvalidOperationException($"Expected offset {zeroBasedOffset} from reply: received {receivedOffset} instead");

                    returnedValue = readValueFunc(reader);
                });

            if (requestContext.TransactionIdentifier != responseContext.TransactionIdentifier)
            {
                throw new InvalidOperationException($"Transaction identifier mismatch: request identifier is {requestContext.TransactionIdentifier} while response identifier is {responseContext.TransactionIdentifier}");
            }

            return returnedValue!;

        }

        private async Task<T> WriteAsync<T>(ModbusDevice device, ModbusFunctionCode functionCode, int zeroBasedOffset, Action<IMessageBufferWriter> writeAction, Func<IMessageBufferReader, Task<T>> readValueFunc, CancellationToken cancellationToken) where T : struct
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
                    writeAction(writer);
                }, cancellationToken);

            T returnedValue = default;

            var responseContext = await ModbusTransport.ReceiveMessageAsync(
                async (reader) =>
                {
                    if (await reader.PushByteFromStreamAsync(cancellationToken) != device.Address)
                        throw new InvalidOperationException($"Unable to read the address ({device.Address}) from the reply");

                    byte receivedFunctionCode = await reader.PushByteFromStreamAsync(cancellationToken);

                    if (receivedFunctionCode == ((byte)functionCode | 0x80))
                    {
                        var exceptionCode = (ModbusExceptionCode)await reader.PushByteFromStreamAsync(cancellationToken);
                        throw new ModbusException(exceptionCode);
                    }

                    if (receivedFunctionCode != (byte)functionCode)
                        throw new InvalidOperationException($"Expected function {functionCode} from reply: received {receivedFunctionCode} instead");

                    var receivedOffset = await reader.PushShortFromStreamAsync(cancellationToken);
                    if (receivedOffset != zeroBasedOffset)
                        throw new InvalidOperationException($"Expected offset {zeroBasedOffset} from reply: received {receivedOffset} instead");

                    returnedValue = await readValueFunc(reader);
                }, cancellationToken);

            if (requestContext.TransactionIdentifier != responseContext.TransactionIdentifier)
            {
                throw new InvalidOperationException($"Transaction identifier mismatch: request identifier is {requestContext.TransactionIdentifier} while response identifier is {responseContext.TransactionIdentifier}");
            }

            return returnedValue;
        }
    }
}
