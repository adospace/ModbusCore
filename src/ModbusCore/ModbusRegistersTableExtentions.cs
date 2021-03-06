﻿using System;

namespace ModbusCore
{
    public static class ModbusRegistersTableExtentions
    {
        internal static void CopyTo(this IModbusRegistersTable table, IMessageBufferWriter bufferWriter, int offset, int count)
        {
            bufferWriter.Push((byte)(count * 2));

            for (var i = 0; i < count; i++)
            {
                bufferWriter.Push((byte)((table[i + offset] >> 8) & 0xFF));
                bufferWriter.Push((byte)(table[i + offset] & 0xFF));
            }
        }

        internal static void CopyTo(this int[] table, IMessageBufferWriter bufferWriter)
        {
            bufferWriter.Push((byte)(table.Length * 2));

            for (var i = 0; i < table.Length; i++)
            {
                bufferWriter.Push((byte)((table[i] >> 8) & 0xFF));
                bufferWriter.Push((byte)(table[i] & 0xFF));
            }
        }

        internal static void CopyFrom(this IModbusRegistersTable table, MessageBufferSpan messageBufferSpan, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                table[i + offset] = (ushort)((messageBufferSpan[i * 2] << 8) + messageBufferSpan[i * 2 + 1] & 0xFFFF);
            }
        }
    }
}