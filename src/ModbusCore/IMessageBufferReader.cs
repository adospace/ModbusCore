using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public interface IMessageBufferReader : IDisposable
    {
        MessageBuffer Buffer { get; }

        byte PushByteFromStream();

        void PushFromStream(int byteCountToReadFromStreamAndPushToBuffer);

        Task<byte> PushByteFromStreamAsync(CancellationToken cancellationToken);

        Task PushFromStreamAsync(int byteCountToReadFromStreamAndPushToBuffer, CancellationToken cancellationToken);

        int Length { get; }

        void Log(IPacketLogger packetLogger);
    }


    public static class MessageBufferReaderExtensions
    {
        public static int PushShortFromStream(this IMessageBufferReader reader)
            => (int)((reader.PushByteFromStream() << 8) + (reader.PushByteFromStream() << 0));
    }
}