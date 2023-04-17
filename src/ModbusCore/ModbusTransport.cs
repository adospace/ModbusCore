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
        public Stream Stream { get; }
        public IPacketLogger? PacketLogger { get; }

        protected readonly MessageBuffer _messageBuffer = new MessageBuffer();

        protected ModbusTransport(Stream stream, IPacketLogger? packetLogger = null)
        {
            Stream = stream;
            PacketLogger = packetLogger;
        }

        protected virtual IMessageBufferReader CreateBufferReader() => _messageBuffer.BeginRead(Stream);
        
        protected virtual IMessageBufferWriter CreateBufferWriter() => _messageBuffer.BeginWrite();

        protected virtual void OnBeginReceivingMessage(IMessageBufferReader messageBufferReader, ModbusTransportContext context)
        {

        }

        protected virtual void OnEndReceivingMessage(IMessageBufferReader messageBufferReader, ModbusTransportContext context)
        {

        }

        protected virtual Task OnBeginReceivingMessageAsync(IMessageBufferReader messageBufferReader, ModbusTransportContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnEndReceivingMessageAsync(IMessageBufferReader messageBufferReader, ModbusTransportContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual void OnBeginSendingMessage(IMessageBufferWriter messageBufferWriter, ModbusTransportContext context)
        {

        }

        protected virtual void OnEndSendingMessage(IMessageBufferWriter messageBufferWriter, ModbusTransportContext context)
        {

        }

        protected virtual Task OnEndSendingMessageAsync(IMessageBufferWriter messageBufferWriter, ModbusTransportContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public ModbusTransportContext ReceiveMessage(Action<IMessageBufferReader> buildMessageFunc)
        {
            using var reader = CreateBufferReader();

            var context = new ModbusTransportContext();

            OnBeginReceivingMessage(reader, context);

            buildMessageFunc(reader);

            OnEndReceivingMessage(reader, context);

            return context;
        }

        public async Task<ModbusTransportContext> ReceiveMessageAsync(Func<IMessageBufferReader, Task> buildMessageFunc, CancellationToken cancellationToken)
        {
            using var reader = CreateBufferReader();

            var context = new ModbusTransportContext();

            await OnBeginReceivingMessageAsync(reader, context, cancellationToken);

            await buildMessageFunc(reader);

            await OnEndReceivingMessageAsync(reader, context, cancellationToken);

            return context;
        }

        public void SendMessage(ModbusTransportContext context, Action<IMessageBufferWriter> buildMessageAction)
        {
            using var writer = CreateBufferWriter();

            OnBeginSendingMessage(writer, context);

            buildMessageAction(writer);

            OnEndSendingMessage(writer, context);
        }

        public async Task SendMessageAsync(ModbusTransportContext context, Action<IMessageBufferWriter> buildMessageAction, CancellationToken cancellationToken)
        {
            using var writer = CreateBufferWriter();

            OnBeginSendingMessage(writer, context);

            buildMessageAction(writer);

            await OnEndSendingMessageAsync(writer, context, cancellationToken);
        }
    }
}
