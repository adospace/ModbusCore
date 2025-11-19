using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public class ModbusAsciiTransport : ModbusTransport
    {
        /// <summary> The start of frame marker for Modbus ASCII messages.</summary>
        public const byte START_FRAME_MARKER = (byte)(0x3A); // ':'

        /// <summary> The first character of the end of frame marker for 
        /// Modbus ASCII messages.
        /// </summary>
        public const byte END_FRAME_MARKER_1 = (byte)(0x0D); // CR

        /// <summary> The second character of the end of frame marker for 
        /// Modbus ASCII messages.
        /// </summary>
        public const byte END_FRAME_MARKER_2 = (byte)(0x0A); // LF

        public ModbusAsciiTransport(Stream stream, IPacketLogger? packetLogger = null) : base(stream, packetLogger)
        {
        }

        protected override IMessageBufferReader CreateBufferReader()
        {
            return new AsciiMessageBufferReader(_messageBuffer, Stream);
        }

        protected override void OnEndReceivingMessage(IMessageBufferReader messageBufferReader, ModbusTransportContext context)
        {
            // Validate LRC
            ValidateLrc();

            PacketLogger?.ReceivedPacket(_messageBuffer.GetBuffer());

            base.OnEndReceivingMessage(messageBufferReader, context);
        }

        protected override async Task OnEndReceivingMessageAsync(IMessageBufferReader messageBufferReader, ModbusTransportContext context, CancellationToken cancellationToken)
        {
            // Validate LRC
            ValidateLrc();

            PacketLogger?.ReceivedPacket(_messageBuffer.GetBuffer());

            await base.OnEndReceivingMessageAsync(messageBufferReader, context, cancellationToken);
        }

        protected override void OnEndSendingMessage(IMessageBufferWriter messageBufferWriter, ModbusTransportContext context)
        {
            // Calculate and append LRC
            AppendLrc(messageBufferWriter);

            // Convert to ASCII and send
            SendAsciiFrame();

            PacketLogger?.SendingPacket(_messageBuffer.GetBuffer());

            base.OnEndSendingMessage(messageBufferWriter, context);
        }

        protected override async Task OnEndSendingMessageAsync(IMessageBufferWriter messageBufferWriter, ModbusTransportContext context, CancellationToken cancellationToken)
        {
            // Calculate and append LRC
            AppendLrc(messageBufferWriter);

            // Convert to ASCII and send
            await SendAsciiFrameAsync(cancellationToken);

            PacketLogger?.SendingPacket(_messageBuffer.GetBuffer());

            await base.OnEndSendingMessageAsync(messageBufferWriter, context, cancellationToken);
        }

        private void SendAsciiFrame()
        {
            // Start frame marker
            Stream.WriteByte(START_FRAME_MARKER);
            
            // Convert each byte to ASCII hex and write
            for (int i = 0; i < _messageBuffer.Length; i++)
            {
                var asciiHex = AsciiUtils.ByteToAsciiHex(_messageBuffer[i]);
                Stream.WriteByte(asciiHex[0]);
                Stream.WriteByte(asciiHex[1]);
            }
            
            // End frame markers
            Stream.WriteByte(END_FRAME_MARKER_1);
            Stream.WriteByte(END_FRAME_MARKER_2);
        }

        private async Task SendAsciiFrameAsync(CancellationToken cancellationToken)
        {
            var frameBytes = new List<byte>();
            
            // Start frame marker
            frameBytes.Add(START_FRAME_MARKER);
            
            // Convert each byte to ASCII hex
            for (int i = 0; i < _messageBuffer.Length; i++)
            {
                var asciiHex = AsciiUtils.ByteToAsciiHex(_messageBuffer[i]);
                frameBytes.Add(asciiHex[0]);
                frameBytes.Add(asciiHex[1]);
            }
            
            // End frame markers
            frameBytes.Add(END_FRAME_MARKER_1);
            frameBytes.Add(END_FRAME_MARKER_2);
            
            await Stream.WriteAsync(frameBytes.ToArray(), 0, frameBytes.Count, cancellationToken);
        }

        private void AppendLrc(IMessageBufferWriter messageBufferWriter)
        {
            var lrc = AsciiUtils.LRC(messageBufferWriter.Buffer);
            messageBufferWriter.Push(lrc);
        }

        private void ValidateLrc()
        {
            if (_messageBuffer.Length < 1)
            {
                throw new ModbusInvalidLrcException("Message too short to contain LRC");
            }

            // Extract received LRC (last byte)
            byte receivedLrc = _messageBuffer[_messageBuffer.Length - 1];
            
            // Calculate expected LRC for message without LRC
            byte expectedLrc = 0;
            for (int i = 0; i < _messageBuffer.Length - 1; i++)
            {
                expectedLrc += _messageBuffer[i];
            }
            expectedLrc = (byte)(-expectedLrc);

            if (receivedLrc != expectedLrc)
            {
                throw new ModbusInvalidLrcException($"Invalid LRC: expected {expectedLrc:X2}, received {receivedLrc:X2}");
            }
        }

        private class AsciiMessageBufferReader : IMessageBufferReader
        {
            private readonly MessageBuffer _owner;
            private readonly Stream _stream;
            private int _readPosition = 0;

            public MessageBuffer Buffer => _owner;
            public int Length => _owner.Length;

            public AsciiMessageBufferReader(MessageBuffer owner, Stream stream)
            {
                _owner = owner;
                _stream = stream;
                
                // Clear the buffer and read the complete ASCII frame
                _owner.Clear();
                ReadCompleteAsciiFrame();
            }

            public byte PushByteFromStream()
            {
                if (_readPosition < _owner.Length)
                {
                    return _owner[_readPosition++];
                }
                throw new InvalidOperationException("No more bytes available in the message buffer");
            }

            public void PushFromStream(int byteCountToReadFromStreamAndPushToBuffer)
            {
                // For ASCII transport, we've already read the complete message
                // This method should advance the read position
                _readPosition += byteCountToReadFromStreamAndPushToBuffer;
            }

            public async Task<byte> PushByteFromStreamAsync(CancellationToken cancellationToken)
            {
                return PushByteFromStream();
            }

            public Task PushFromStreamAsync(int byteCountToReadFromStreamAndPushToBuffer, CancellationToken cancellationToken)
            {
                PushFromStream(byteCountToReadFromStreamAndPushToBuffer);
                return Task.CompletedTask;
            }

            private void ReadCompleteAsciiFrame()
            {
                var asciiBytes = new List<byte>();
                
                // Read until start marker
                byte currentByte;
                do
                {
                    int b = _stream.ReadByte();
                    if (b == -1) throw new EndOfStreamException();
                    currentByte = (byte)b;
                } while (currentByte != START_FRAME_MARKER);

                // Read ASCII hex characters until CR
                do
                {
                    int b = _stream.ReadByte();
                    if (b == -1) throw new EndOfStreamException();
                    currentByte = (byte)b;
                    
                    if (currentByte != END_FRAME_MARKER_1)
                    {
                        asciiBytes.Add(currentByte);
                    }
                } while (currentByte != END_FRAME_MARKER_1);

                // Read LF
                int lfByte = _stream.ReadByte();
                if (lfByte == -1) throw new EndOfStreamException();
                if (lfByte != END_FRAME_MARKER_2)
                {
                    throw new ModbusException("Expected LF after CR in ASCII frame");
                }

                // Convert ASCII hex to binary
                ConvertAsciiHexToBinary(asciiBytes);
            }

            private void ConvertAsciiHexToBinary(List<byte> asciiHexBytes)
            {                
                if (asciiHexBytes.Count % 2 != 0)
                {
                    throw new ModbusException("Invalid ASCII hex frame - odd number of characters");
                }

                using var writer = _owner.BeginWrite();
                
                for (int i = 0; i < asciiHexBytes.Count; i += 2)
                {
                    byte binaryByte = AsciiUtils.AsciiHexPairToByte(asciiHexBytes[i], asciiHexBytes[i + 1]);
                    writer.Push(binaryByte);
                }
            }

            public void Dispose()
            {
                // Nothing to dispose
            }
        }
    }
}