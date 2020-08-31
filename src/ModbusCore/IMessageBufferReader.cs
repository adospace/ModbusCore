using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public interface IMessageBufferReader : IDisposable
    {
        byte PushByteFromStream();

        void PushFromStream(int byteCountToReadFromStreamAndPushToBuffer);

        Task<byte> PushByteFromStreamAsync(CancellationToken cancellationToken);

        Task PushFromStreamAsync(int byteCountToReadFromStreamAndPushToBuffer, CancellationToken cancellationToken);

        int Length { get; }

        void Log(IPacketLogger packetLogger);
    }
}