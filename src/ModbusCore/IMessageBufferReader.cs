using System;

namespace ModbusCore
{
    public interface IMessageBufferReader : IDisposable
    {
        byte PushFromStream();

        void PushFromStream(int byteCountToReadFromStreamAndPushToBuffer);

        int Length { get; }
    }
}