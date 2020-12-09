using System;

namespace ModbusCore
{
    public interface IMessageBufferWriter : IDisposable
    {
        MessageBuffer Buffer { get; }

        void Push(byte value);

        int Length { get; }

        void Log(IPacketLogger packetLogger);
    }
}