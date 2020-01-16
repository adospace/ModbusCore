﻿using System;
using System.IO;

namespace ModbusCore
{
    public class ModbusRTUSlave : ModbusDevice
    {
        private readonly MessageBuffer _messageBuffer = new MessageBuffer();

        public ModbusRTUSlave(ModbusMemoryMap memoryMap, Stream stream, byte address)
            : base(memoryMap, stream, address)
        {
        }

        public void HandleAnyRequest()
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            var functionCode = (ModbusFunctionCode)messageBufferReader.PushFromStream();

            switch (functionCode)
            {
                case ModbusFunctionCode.ReadDiscreteInputs:
                    break;

                case ModbusFunctionCode.ReadCoils:
                    HandleReadCoilsRequest(messageBufferReader, out var offset, out var count);
                    HandleReadCoilsResponse(offset, count);
                    return;

                case ModbusFunctionCode.WriteSingleCoil:
                    break;

                case ModbusFunctionCode.WriteMultipleCoils:
                    break;

                case ModbusFunctionCode.ReadInputRegister:
                    break;

                case ModbusFunctionCode.ReadHoldingRegisters:
                    break;

                case ModbusFunctionCode.WriteSingleRegister:
                    break;

                case ModbusFunctionCode.WriteMultipleRegisters:
                    break;

                case ModbusFunctionCode.ReadWriteMultipleRegisters:
                    break;

                case ModbusFunctionCode.MaskWriteRegister:
                    break;

                case ModbusFunctionCode.ReadFiFoQueue:
                    break;

                case ModbusFunctionCode.ReadFileRecord:
                    break;

                case ModbusFunctionCode.WriteFileRecord:
                    break;

                case ModbusFunctionCode.ReadExceptionStatus:
                    break;

                case ModbusFunctionCode.Diagnostic:
                    break;

                case ModbusFunctionCode.GetComEventCounter:
                    break;

                case ModbusFunctionCode.GetComEventLog:
                    break;

                case ModbusFunctionCode.ReportServerID:
                    break;

                case ModbusFunctionCode.ReadDeviceIdentification:
                    break;
            }

            throw new NotImplementedException($"Function '{functionCode}' is not implemented");
        }

        internal void HandleReadCoilsRequest(out int zeroBasedOffset, out int count)
        {
            using var messageBufferReader = _messageBuffer.BeginRead(Stream);

            if (messageBufferReader.PushFromStream() != Address)
                throw new InvalidOperationException();

            if (messageBufferReader.PushFromStream() != (byte)ModbusFunctionCode.ReadCoils)
                throw new InvalidOperationException();

            HandleReadCoilsRequest(messageBufferReader, out zeroBasedOffset, out count);
        }

        internal void HandleReadCoilsResponse(int zeroBasedOffset, int count)
        {
            //write the response
            using var messageBufferWriter = _messageBuffer.BeginWrite();
            messageBufferWriter.Push((byte)((Address >> 0) & 0xFF));
            messageBufferWriter.Push((byte)ModbusFunctionCode.ReadCoils);

            MemoryMap.InputCoils.CopyTo(messageBufferWriter, zeroBasedOffset, count);

            AppendCrcToResponse(messageBufferWriter);

            _messageBuffer.WriteToStream(Stream);
        }

        private void AppendCrcToResponse(IMessageBufferWriter messageBufferWriter)
        {
            var crc = CrcUtils.CRC16(_messageBuffer);

            // Write the high nibble of the CRC
            messageBufferWriter.Push((byte)(((crc & 0xFF00) >> 8) & 0xFF));

            // Write the low nibble of the CRC
            messageBufferWriter.Push((byte)((crc & 0x00FF) & 0xFF));
        }

        private void CheckCrcIsValidFromRequest(IMessageBufferReader messageBufferReader)
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
        private void HandleReadCoilsRequest(IMessageBufferReader messageBufferReader, out int zeroBasedOffset, out int count)
        {
            zeroBasedOffset = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));
            count = (ushort)((messageBufferReader.PushFromStream() << 8) + (messageBufferReader.PushFromStream() << 0));

            CheckCrcIsValidFromRequest(messageBufferReader);
        }
    }
}