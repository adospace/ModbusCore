using System;

namespace ModbusCore
{
    internal static class Validate
    {
        public static void Between(string parameterName, int parameterValue, int from, int to)
        {
            if (from > to && parameterValue < from)
                throw new ArgumentException(parameterName, $"'{parameterName}' argument must be greater or equal to {from}");
            if (parameterValue < from || parameterValue > to)
                throw new ArgumentException(parameterName, $"'{parameterName}' argument must be a value between {from} and {to}");
        }
    }
}