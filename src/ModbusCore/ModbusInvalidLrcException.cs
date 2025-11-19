using System;

namespace ModbusCore
{
    /// <summary>
    /// Exception thrown when an invalid LRC (Longitudinal Redundancy Check) is detected in a Modbus ASCII message
    /// </summary>
    internal class ModbusInvalidLrcException : ModbusException
    {
        public ModbusInvalidLrcException(string message) : base(message)
        {
        }

        public ModbusInvalidLrcException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}