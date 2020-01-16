using System;
using System.Runtime.Serialization;

namespace ModbusCore
{
    [Serializable]
    internal class ModbusException : Exception
    {
        public ModbusException()
        {
        }

        public ModbusException(ModbusExceptionCode exceptionCode)
        {
            this.ExceptionCode = exceptionCode;
        }

        public ModbusException(string message) : base(message)
        {
        }

        public ModbusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ModbusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ModbusExceptionCode ExceptionCode { get; }
    }
}