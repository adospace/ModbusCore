using System;

namespace ModbusCore
{
    /// <summary>
    /// Utility class for Modbus ASCII transport operations
    /// </summary>
    public static class AsciiUtils
    {
        /// <summary>
        /// Calculate LRC (Longitudinal Redundancy Check) for Modbus ASCII messages
        /// </summary>
        /// <param name="messageBuffer">The message buffer containing the data to calculate LRC for</param>
        /// <returns>The LRC value as a byte</returns>
        public static byte LRC(MessageBuffer messageBuffer)
        {
            byte lrc = 0;
            for (int i = 0; i < messageBuffer.Length; i++)
            {
                lrc += messageBuffer[i];
            }
            return (byte)(-lrc);
        }

        /// <summary>
        /// Convert a byte value to its ASCII hex representation (2 characters)
        /// </summary>
        /// <param name="value">The byte value to convert</param>
        /// <returns>Array of 2 bytes representing the ASCII hex characters</returns>
        public static byte[] ByteToAsciiHex(byte value)
        {
            var result = new byte[2];
            result[0] = NibbleToAsciiHex((byte)((value >> 4) & 0x0F));
            result[1] = NibbleToAsciiHex((byte)(value & 0x0F));
            return result;
        }

        /// <summary>
        /// Convert a nibble (4-bit value) to its ASCII hex character
        /// </summary>
        /// <param name="nibble">The nibble value (0-15)</param>
        /// <returns>The ASCII hex character byte</returns>
        public static byte NibbleToAsciiHex(byte nibble)
        {
            return (byte)(nibble < 10 ? nibble + '0' : nibble - 10 + 'A');
        }

        /// <summary>
        /// Convert ASCII hex character to its byte value
        /// </summary>
        /// <param name="asciiHexChar">The ASCII hex character</param>
        /// <returns>The byte value (0-15)</returns>
        public static byte AsciiHexToByte(byte asciiHexChar)
        {
            if (asciiHexChar >= '0' && asciiHexChar <= '9')
                return (byte)(asciiHexChar - '0');
            if (asciiHexChar >= 'A' && asciiHexChar <= 'F')
                return (byte)(asciiHexChar - 'A' + 10);
            if (asciiHexChar >= 'a' && asciiHexChar <= 'f')
                return (byte)(asciiHexChar - 'a' + 10);
            
            throw new ArgumentException($"Invalid ASCII hex character: {(char)asciiHexChar}");
        }

        /// <summary>
        /// Convert two ASCII hex characters to a single byte value
        /// </summary>
        /// <param name="highNibble">The high nibble ASCII hex character</param>
        /// <param name="lowNibble">The low nibble ASCII hex character</param>
        /// <returns>The combined byte value</returns>
        public static byte AsciiHexPairToByte(byte highNibble, byte lowNibble)
        {
            return (byte)((AsciiHexToByte(highNibble) << 4) | AsciiHexToByte(lowNibble));
        }
    }
}