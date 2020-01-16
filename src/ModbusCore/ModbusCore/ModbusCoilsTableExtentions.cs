﻿using System;

namespace ModbusCore
{
    public static class ModbusCoilsTableExtentions
    {
        internal static void CopyTo(this IModbusCoilsTable table, IMessageBufferWriter bufferWriter, int offset, int count)
        {
            var n = Math.DivRem(count, 8, out var res);
            if (res > 0)
                n++;

            bufferWriter.Push((byte)n);

            for (var i = 0; i < n; i++)
            {
                byte currentByte = 0;
                for (var j = 0; j < 8; j++)
                {
                    if (table[i * 8 + j + offset])
                        currentByte |= (byte)(0x1 << j);
                }

                bufferWriter.Push(currentByte);
            }
        }

        internal static void CopyFrom(this IModbusCoilsTable table, MessageBufferSpan messageBufferSpan, int offset, int count)
        {
            var n = Math.DivRem(count, 8, out var res);
            if (res > 0)
                n++;

            for (var i = 0; i < n; i++)
            {
                byte currentByte = messageBufferSpan[i];
                for (var j = 0; j < 8; j++)
                {
                    table[i * 8 + j + offset] =
                        ((currentByte & (byte)(0x1 << j)) > 0);
                }
            }
        }
    }
}