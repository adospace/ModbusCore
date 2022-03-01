using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public class ModbusRTUTransport : ModbusTransport
    {
        public ModbusRTUTransport(Stream stream, IPacketLogger? packetLogger = null) : base(stream, packetLogger)
        {
        }

        protected override void OnEndReceivingMessage(IMessageBufferReader messageBufferReader, ModbusTransportContext context)
        {
            var messageCrc = CrcUtils.CRC16(messageBufferReader.Buffer);

            messageBufferReader.PushFromStream(2);

            PacketLogger?.ReceivedPacket(_messageBuffer.GetBuffer());

            CheckCrcIsValid(messageBufferReader, messageCrc);

            base.OnEndReceivingMessage(messageBufferReader, context);
        }

        protected override async Task OnEndReceivingMessageAsync(IMessageBufferReader messageBufferReader, ModbusTransportContext context, CancellationToken cancellationToken)
        {
            var messageCrc = CrcUtils.CRC16(messageBufferReader.Buffer);

            //check CRC
            await messageBufferReader.PushFromStreamAsync(2, cancellationToken);

            PacketLogger?.ReceivedPacket(_messageBuffer.GetBuffer());

            CheckCrcIsValid(messageBufferReader, messageCrc);
        }

        protected override void OnEndSendingMessage(IMessageBufferWriter messageBufferWriter, ModbusTransportContext context)
        {
            AppendCrc(messageBufferWriter);

            PacketLogger?.SendingPacket(_messageBuffer.GetBuffer());

            _messageBuffer.WriteToStream(Stream);

            base.OnEndSendingMessage(messageBufferWriter, context);
        }

        protected override async Task OnEndSendingMessageAsync(IMessageBufferWriter messageBufferWriter, ModbusTransportContext context, CancellationToken cancellationToken)
        {
            AppendCrc(messageBufferWriter);

            PacketLogger?.SendingPacket(_messageBuffer.GetBuffer());

            await _messageBuffer.WriteToStreamAsync(Stream, cancellationToken);
        }

        protected void AppendCrc(IMessageBufferWriter messageBufferWriter)
        {
            var crc = CrcUtils.CRC16(messageBufferWriter.Buffer);

            // Write the high nibble of the CRC
            messageBufferWriter.Push((byte)(((crc & 0xFF00) >> 8) & 0xFF));

            // Write the low nibble of the CRC
            messageBufferWriter.Push((byte)((crc & 0x00FF) & 0xFF));
        }

        protected void CheckCrcIsValid(IMessageBufferReader messageBufferReader, ushort messageCrc)
        {
            var messageBuffer = messageBufferReader.Buffer;

            byte recvCRC1 = messageBuffer[messageBuffer.Length - 2];
            byte rectCRC2 = messageBuffer[messageBuffer.Length - 1];

            if (recvCRC1 != (byte)(((messageCrc & 0xFF00) >> 8) & 0xFF) ||
                rectCRC2 != (byte)((messageCrc & 0x00FF) & 0xFF))
            {
                throw new ModbusInvalidCRCException($"Invalid CRC: expected {(byte)(((messageCrc & 0xFF00) >> 8) & 0xFF):X2} {(byte)(messageCrc & 0x00FF & 0xFF):X2} received {recvCRC1:X2} {rectCRC2:X2}");
            }
        }
    }
}
