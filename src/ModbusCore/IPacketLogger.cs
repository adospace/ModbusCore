using System;

namespace ModbusCore
{
    public interface IPacketLogger
    {
        void SendingPacket(ReadOnlySpan<byte> data);

        void ReceivedPacket(ReadOnlySpan<byte> data);
    }
}