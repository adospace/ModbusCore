using System;

namespace ModbusCore
{
    public static class ModbusMasterExtensions
    {
        public static void ReadCoilsSpan(this ModbusTransport master, int zeroBasedStartOffset, int zeroBasedEndOffset)
        {
            if (master is null)
            {
                throw new ArgumentNullException(nameof(master));
            }

            master.ReadCoils(zeroBasedStartOffset, zeroBasedEndOffset - zeroBasedStartOffset + 1);
        }
    }
}