﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public class ModbusServer
    {
        public ModbusServer(
            ModbusTransport modbusTransport,
            IPacketLogger? packetLogger = null)
        {
            ModbusTransport = modbusTransport;
            PacketLogger = packetLogger;
        }

        internal ModbusTransport ModbusTransport { get; }
        public IPacketLogger? PacketLogger { get; }
        protected virtual int GetTransactionIdentifier() => 0;

        public abstract class MessageHandler
        {
            public MessageHandler()
            { }

            public abstract void WriteResponse(ModbusDevice device, IMessageBufferWriter writer);
            protected abstract void Initialize(IMessageBufferReader reader);
            protected abstract Task InitializeAsync(IMessageBufferReader reader, CancellationToken cancellationToken);
        }

        public abstract class ReadHandler : MessageHandler
        {
            public ReadHandler()
            {
            }

            public int Offset { get; private set; }
            public int Count { get; private set; }

            protected override void Initialize(IMessageBufferReader reader)
            {
                Offset = reader.PushShortFromStream();
                Count = reader.PushShortFromStream();
            }

            protected override async Task InitializeAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                Offset = await reader.PushShortFromStreamAsync(cancellationToken);
                Count = await reader.PushShortFromStreamAsync(cancellationToken);
            }
        }

        public class ReadCoilsHandler : ReadHandler
        {
            public ReadCoilsHandler()
            {
            }

            public override void WriteResponse(ModbusDevice device, IMessageBufferWriter writer)
            {
                writer.Push((byte)((device.Address >> 0) & 0xFF));
                writer.Push((byte)ModbusFunctionCode.ReadCoils);

                device.MemoryMap.OutputCoils.CopyTo(writer, Offset, Count);
            }

            public static ReadCoilsHandler Create(IMessageBufferReader reader)
            {
                var handler = new ReadCoilsHandler();
                handler.Initialize(reader);
                return handler;
            }

            public static async Task<ReadCoilsHandler> CreateAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                var handler = new ReadCoilsHandler();
                await handler.InitializeAsync(reader, cancellationToken);
                return handler;
            }
        }

        public class ReadDiscreteInputsHandler : ReadHandler
        {
            public ReadDiscreteInputsHandler()
            {
            }

            public override void WriteResponse(ModbusDevice device, IMessageBufferWriter writer)
            {
                writer.Push((byte)((device.Address >> 0) & 0xFF));
                writer.Push((byte)ModbusFunctionCode.ReadDiscreteInputs);

                device.MemoryMap.InputCoils.CopyTo(writer, Offset, Count);
            }

            public static ReadDiscreteInputsHandler Create(IMessageBufferReader reader)
            {
                var handler = new ReadDiscreteInputsHandler();
                handler.Initialize(reader);
                return handler;
            }

            public static async Task<ReadDiscreteInputsHandler> CreateAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                var handler = new ReadDiscreteInputsHandler();
                await handler.InitializeAsync(reader, cancellationToken);
                return handler;
            }
        }

        public class ReadInputRegistersHandler : ReadHandler
        {
            public ReadInputRegistersHandler()
            {
            }

            public override void WriteResponse(ModbusDevice device, IMessageBufferWriter writer)
            {
                writer.Push((byte)((device.Address >> 0) & 0xFF));
                writer.Push((byte)ModbusFunctionCode.ReadInputRegisters);

                device.MemoryMap.InputRegisters.CopyTo(writer, Offset, Count);
            }

            public static ReadInputRegistersHandler Create(IMessageBufferReader reader)
            {
                var handler = new ReadInputRegistersHandler();
                handler.Initialize(reader);
                return handler;
            }

            public static async Task<ReadInputRegistersHandler> CreateAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                var handler = new ReadInputRegistersHandler();
                await handler.InitializeAsync(reader, cancellationToken);
                return handler;
            }
        }

        public class ReadHoldingRegistersHandler : ReadHandler
        {
            public ReadHoldingRegistersHandler()
            {
            }

            public override void WriteResponse(ModbusDevice device, IMessageBufferWriter writer)
            {
                writer.Push((byte)((device.Address >> 0) & 0xFF));
                writer.Push((byte)ModbusFunctionCode.ReadHoldingRegisters);

                device.MemoryMap.OutputRegisters.CopyTo(writer, Offset, Count);
            }

            public static ReadHoldingRegistersHandler Create(IMessageBufferReader reader)
            {
                var handler = new ReadHoldingRegistersHandler();
                handler.Initialize(reader);
                return handler;
            }

            public static async Task<ReadHoldingRegistersHandler> CreateAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                var handler = new ReadHoldingRegistersHandler();
                await handler.InitializeAsync(reader, cancellationToken);
                return handler;
            }
        }

        public abstract class WriteHandler : MessageHandler
        {
            protected WriteHandler()
            {
            }

            protected override void Initialize(IMessageBufferReader reader)
            { 
                Offset = (ushort)((reader.PushByteFromStream() << 8) +
                    (reader.PushByteFromStream() << 0));
            }

            protected override async Task InitializeAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                Offset = await reader.PushShortFromStreamAsync(cancellationToken);
            }

            public int Offset { get; private set; }

            public abstract void UpdateMemory(ModbusDevice device);
        }

        public class WriteSingleCoilHandler : WriteHandler
        {
            public WriteSingleCoilHandler()
            {
            }

            public bool Value { get; private set; }

            protected override void Initialize(IMessageBufferReader reader)
            {
                base.Initialize(reader);

                Value = reader.PushByteFromStream() == 0xFF;

                if (reader.PushByteFromStream() != 0x00)
                    throw new InvalidOperationException();
            }

            protected override async Task InitializeAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                await base.InitializeAsync(reader, cancellationToken);

                Value = await reader.PushByteFromStreamAsync(cancellationToken) == 0xFF;

                if (await reader.PushByteFromStreamAsync(cancellationToken) != 0x00)
                    throw new InvalidOperationException();
            }

            public static WriteSingleCoilHandler Create(IMessageBufferReader reader)
            {
                var handler = new WriteSingleCoilHandler();
                handler.Initialize(reader);
                return handler;
            }

            public static async Task<WriteSingleCoilHandler> CreateAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                var handler = new WriteSingleCoilHandler();
                await handler.InitializeAsync(reader, cancellationToken);
                return handler;
            }

            public override void UpdateMemory(ModbusDevice device)
            {
                device.MemoryMap.OutputCoils[Offset] = Value;
            }

            public override void WriteResponse(ModbusDevice device, IMessageBufferWriter writer)
            {
                writer.Push((byte)((device.Address >> 0) & 0xFF));
                writer.Push((byte)ModbusFunctionCode.WriteSingleCoil);

                writer.Push((byte)((Offset >> 8) & 0xFF));
                writer.Push((byte)((Offset >> 0) & 0xFF));
                writer.Push((byte)(device.MemoryMap.OutputCoils[Offset] ? 0xFF : 0x00));
                writer.Push(0);
            }
        }

        public class WriteSingleRegisterHandler : WriteHandler
        {
            protected WriteSingleRegisterHandler()
            {
            }

            protected override void Initialize(IMessageBufferReader reader)
            {
                base.Initialize(reader);

                Value = reader.PushShortFromStream();
            }

            protected override  async Task InitializeAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                await base.InitializeAsync(reader, cancellationToken);

                Value = await reader.PushShortFromStreamAsync(cancellationToken);
            }

            public static WriteSingleRegisterHandler Create(IMessageBufferReader reader)
            {
                var handler = new WriteSingleRegisterHandler();
                handler.Initialize(reader);
                return handler;
            }

            public static async Task<WriteSingleRegisterHandler> CreateAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                var handler = new WriteSingleRegisterHandler();
                await handler.InitializeAsync(reader, cancellationToken);
                return handler;
            }

            public int Value { get; private set; }

            public override void UpdateMemory(ModbusDevice device)
            {
                device.MemoryMap.OutputRegisters[Offset] = Value;
            }

            public override void WriteResponse(ModbusDevice device, IMessageBufferWriter writer)
            {
                writer.Push((byte)((device.Address >> 0) & 0xFF));
                writer.Push((byte)ModbusFunctionCode.WriteSingleRegister);

                writer.Push((byte)((Offset >> 8) & 0xFF));
                writer.Push((byte)((Offset >> 0) & 0xFF));
                writer.Push((byte)((device.MemoryMap.OutputRegisters[Offset] >> 8) & 0xFF));
                writer.Push((byte)((device.MemoryMap.OutputRegisters[Offset] >> 0) & 0xFF));
            }
        }

        public abstract class WriteMultipleHandler : WriteHandler
        {
            protected WriteMultipleHandler(ModbusFunctionCode functionCode)
            {
                FunctionCode = functionCode;
            }

            protected override void Initialize(IMessageBufferReader reader)
            {
                base.Initialize(reader);

                Count = reader.PushShortFromStream();

                var byteCount = reader.PushByteFromStream();

                reader.PushFromStream(byteCount);

                MessageBuffer = new MessageBufferSpan(reader.Buffer, (ushort)(reader.Buffer.Length - byteCount - 2), byteCount);

            }

            protected override async Task InitializeAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                await base.InitializeAsync(reader, cancellationToken);

                Count = await reader.PushShortFromStreamAsync(cancellationToken);

                var byteCount = await reader.PushByteFromStreamAsync(cancellationToken);

                await reader.PushFromStreamAsync(byteCount, cancellationToken);

                MessageBuffer = new MessageBufferSpan(reader.Buffer, (ushort)(reader.Buffer.Length - byteCount - 2), byteCount);

            }

            public int Count { get; private set; }
            public MessageBufferSpan? MessageBuffer { get; private set; }
            public ModbusFunctionCode FunctionCode { get; }

            public override void WriteResponse(ModbusDevice device, IMessageBufferWriter writer)
            {
                writer.Push((byte)((device.Address >> 0) & 0xFF));
                writer.Push((byte)FunctionCode);

                writer.Push((byte)((Offset >> 8) & 0xFF));
                writer.Push((byte)((Offset >> 0) & 0xFF));
                writer.Push((byte)((Count >> 8) & 0xFF));
                writer.Push((byte)((Count >> 0) & 0xFF));
            }
        }

        public class WriteMultipleCoilsHandler : WriteMultipleHandler
        {
            protected WriteMultipleCoilsHandler()
                :base(ModbusFunctionCode.WriteMultipleCoils)
            {
            }


            public static WriteMultipleCoilsHandler Create(IMessageBufferReader reader)
            {
                var handler = new WriteMultipleCoilsHandler();
                handler.Initialize(reader);
                return handler;
            }

            public static async Task<WriteMultipleCoilsHandler> CreateAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                var handler = new WriteMultipleCoilsHandler();
                await handler.InitializeAsync(reader, cancellationToken);
                return handler;
            }

            public override void UpdateMemory(ModbusDevice device)
            {
                if (MessageBuffer == null)
                {
                    throw new InvalidOperationException();
                }

                device.MemoryMap.OutputCoils.CopyFrom(MessageBuffer, Offset, Count);
            }

        }

        public class WriteMultipleRegistersHandler : WriteMultipleHandler
        {
            protected WriteMultipleRegistersHandler()
                : base(ModbusFunctionCode.WriteMultipleRegisters)
            {
            }

            public static WriteMultipleRegistersHandler Create(IMessageBufferReader reader)
            {
                var handler = new WriteMultipleRegistersHandler();
                handler.Initialize(reader);
                return handler;
            }

            public static async Task<WriteMultipleRegistersHandler> CreateAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                var handler = new WriteMultipleRegistersHandler();
                await handler.InitializeAsync(reader, cancellationToken);
                return handler;
            }

            public override void UpdateMemory(ModbusDevice device)
            {
                if (MessageBuffer == null)
                {
                    throw new InvalidOperationException();
                }

                device.MemoryMap.OutputRegisters.CopyFrom(MessageBuffer, Offset, Count);
            }
        }

        public class ReadWriteMultipleRegistersHandler : WriteHandler
        {
            protected ReadWriteMultipleRegistersHandler()
            {
            }

            public static ReadWriteMultipleRegistersHandler Create(IMessageBufferReader reader)
            {
                var handler = new ReadWriteMultipleRegistersHandler();
                handler.Initialize(reader);
                return handler;
            }

            public static async Task<ReadWriteMultipleRegistersHandler> CreateAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                var handler = new ReadWriteMultipleRegistersHandler();
                await handler.InitializeAsync(reader, cancellationToken);
                return handler;
            }

            protected override void Initialize(IMessageBufferReader reader)
            {
                base.Initialize(reader);

                ReadCount = reader.PushShortFromStream();

                WriteOffset = reader.PushShortFromStream();
                WriteCount = reader.PushShortFromStream();

                var byteCount = reader.PushByteFromStream();

                reader.PushFromStream(byteCount);

                MessageBuffer = new MessageBufferSpan(reader.Buffer, (ushort)(reader.Buffer.Length - byteCount - 2), byteCount);
            }

            protected override async Task InitializeAsync(IMessageBufferReader reader, CancellationToken cancellationToken)
            {
                await base.InitializeAsync(reader, cancellationToken);

                ReadCount = await reader.PushShortFromStreamAsync(cancellationToken);

                WriteOffset = await reader.PushShortFromStreamAsync(cancellationToken);
                WriteCount = await reader.PushShortFromStreamAsync(cancellationToken);

                var byteCount = await reader.PushByteFromStreamAsync(cancellationToken);

                await reader.PushFromStreamAsync(byteCount, cancellationToken);

                MessageBuffer = new MessageBufferSpan(reader.Buffer, (ushort)(reader.Buffer.Length - byteCount - 2), byteCount);
            }

            public int ReadOffset { get; private set; }
            public int ReadCount { get; private set; }

            public int WriteOffset { get; private set; }
            public int WriteCount { get; private set; }

            public MessageBufferSpan? MessageBuffer { get; private set; }
            public ModbusFunctionCode FunctionCode { get; }

            public override void UpdateMemory(ModbusDevice device)
            {
                if (MessageBuffer == null)
                {
                    throw new InvalidOperationException();
                }

                device.MemoryMap.OutputRegisters.CopyFrom(MessageBuffer, WriteOffset, WriteCount);
            }

            public override void WriteResponse(ModbusDevice device, IMessageBufferWriter writer)
            {
                writer.Push((byte)((device.Address >> 0) & 0xFF));
                writer.Push((byte)FunctionCode);

                device.MemoryMap.OutputRegisters.CopyTo(writer, ReadOffset, ReadCount);
            }
        }

        public void HandleRequest(params ModbusDevice[] devices)
        {
            if (devices is null)
            {
                throw new ArgumentNullException(nameof(devices));
            }

            if (devices.Length == 0)
            {
                throw new ArgumentException();
            }

            MessageHandler? messageHandler = null;
            ModbusDevice? device = null;

            var requestContext = ModbusTransport.ReceiveMessage(reader =>
            {
                var deviceAddress = reader.PushByteFromStream();

                device = devices.FirstOrDefault(_ => _.Address == deviceAddress);

                if (device == null)
                {
                    throw new InvalidOperationException($"Received request for device with unknown address {deviceAddress}");
                }

                var functionCode = (ModbusFunctionCode)reader.PushByteFromStream();
                messageHandler = functionCode switch
                {
                    ModbusFunctionCode.ReadDiscreteInputs => ReadDiscreteInputsHandler.Create(reader),
                    ModbusFunctionCode.ReadCoils => ReadCoilsHandler.Create(reader),
                    ModbusFunctionCode.ReadInputRegisters => ReadInputRegistersHandler.Create(reader),
                    ModbusFunctionCode.ReadHoldingRegisters => ReadHoldingRegistersHandler.Create(reader),

                    ModbusFunctionCode.WriteSingleCoil => WriteSingleCoilHandler.Create(reader),
                    ModbusFunctionCode.WriteSingleRegister => WriteSingleRegisterHandler.Create(reader),
                    ModbusFunctionCode.WriteMultipleCoils => WriteMultipleCoilsHandler.Create(reader),
                    ModbusFunctionCode.WriteMultipleRegisters => WriteMultipleRegistersHandler.Create(reader),

                    ModbusFunctionCode.ReadWriteMultipleRegisters => ReadWriteMultipleRegistersHandler.Create(reader),
                    _ => throw new NotSupportedException($"Function {functionCode} not supported"),
                };
            });

            if (messageHandler is WriteHandler writeHandler)
            {
                writeHandler.UpdateMemory(device!);
            }

            ModbusTransport.SendMessage(requestContext, writer => 
            {
                messageHandler!.WriteResponse(device!, writer);            
            });
        }

        public async Task HandleRequestAsync(ModbusDevice[] devices, CancellationToken cancellationToken)
        {
            if (devices is null)
            {
                throw new ArgumentNullException(nameof(devices));
            }

            if (devices.Length == 0)
            {
                throw new ArgumentException();
            }

            MessageHandler? messageHandler = null;
            ModbusDevice? device = null;

            var requestContext = await ModbusTransport.ReceiveMessageAsync(async reader =>
            {
                var deviceAddress = await reader.PushByteFromStreamAsync(cancellationToken);

                device = devices.FirstOrDefault(_ => _.Address == deviceAddress);

                if (device == null)
                {
                    throw new InvalidOperationException($"Received request for device with unknown address {deviceAddress}");
                }

                var functionCode = (ModbusFunctionCode)await reader.PushByteFromStreamAsync(cancellationToken);
                messageHandler = functionCode switch
                {
                    ModbusFunctionCode.ReadDiscreteInputs => await ReadDiscreteInputsHandler.CreateAsync(reader, cancellationToken),
                    ModbusFunctionCode.ReadCoils => await ReadCoilsHandler.CreateAsync(reader, cancellationToken),
                    ModbusFunctionCode.ReadInputRegisters => await ReadInputRegistersHandler.CreateAsync(reader, cancellationToken),
                    ModbusFunctionCode.ReadHoldingRegisters => await ReadHoldingRegistersHandler.CreateAsync(reader, cancellationToken),

                    ModbusFunctionCode.WriteSingleCoil => await WriteSingleCoilHandler.CreateAsync(reader, cancellationToken),
                    ModbusFunctionCode.WriteSingleRegister => await WriteSingleRegisterHandler.CreateAsync(reader, cancellationToken),
                    ModbusFunctionCode.WriteMultipleCoils => await WriteMultipleCoilsHandler.CreateAsync(reader, cancellationToken),
                    ModbusFunctionCode.WriteMultipleRegisters => await WriteMultipleRegistersHandler.CreateAsync(reader, cancellationToken),

                    ModbusFunctionCode.ReadWriteMultipleRegisters => await ReadWriteMultipleRegistersHandler.CreateAsync(reader, cancellationToken),
                    _ => throw new NotSupportedException($"Function {functionCode} not supported"),
                };
            }, cancellationToken);

            if (messageHandler is WriteHandler writeHandler)
            {
                writeHandler.UpdateMemory(device!);
            }

            await ModbusTransport.SendMessageAsync(requestContext, writer =>
            {
                messageHandler!.WriteResponse(device!, writer);
            }, cancellationToken);
        }
    }
}
