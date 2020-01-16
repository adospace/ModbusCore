using System;

namespace ModbusCore
{
    public interface IMessageBufferWriter : IDisposable
    {
        void Push(byte value);

        int Length { get; }
    }
}