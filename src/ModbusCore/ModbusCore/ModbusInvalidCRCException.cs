using System;
using System.Runtime.Serialization;

namespace ModbusCore
{
    [Serializable]
    internal class ModbusInvalidCRCException : Exception
    {
        public ModbusInvalidCRCException()
        {
        }

        public ModbusInvalidCRCException(string message) : base(message)
        {
        }

        public ModbusInvalidCRCException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ModbusInvalidCRCException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}