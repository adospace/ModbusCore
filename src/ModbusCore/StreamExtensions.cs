using System;
using System.IO;

namespace ModbusCore
{
    internal static class StreamExtensions
    {
        public static byte ReadByteEx(this Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            int readByte = stream.ReadByte();
            if (readByte == -1)
                throw new IOException();

            return (byte)readByte;
        }
    }
}