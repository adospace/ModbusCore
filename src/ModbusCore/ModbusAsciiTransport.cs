using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModbusCore
{
    public class ModbusAsciiTransport : ModbusTransport
    {
		/// <summary> The start of frame marker for Modbus ASCII messages.</summary>
		public const byte START_FRAME_MARKER = (byte)(0x3A); // ':'

		/// <summary> The first character of the end of frame marker for 
		/// Modbus ASCII messages.
		/// </summary>
		public const byte END_FRAME_MARKER_1 = (byte)(0x0D); // CR

		/// <summary> The first character of the end of frame marker for 
		/// Modbus ASCII messages.
		/// </summary>
		public const byte END_FRAME_MARKER_2 = (byte)(0x0A); // LF

        public ModbusAsciiTransport(Stream stream, IPacketLogger? packetLogger = null) : base(stream, packetLogger)
        {
        }
    }
}
