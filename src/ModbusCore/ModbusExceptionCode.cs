namespace ModbusCore
{
    public enum ModbusExceptionCode : byte
    {
        None = 0x00,

        InvalidFunction = 0x01,

        InvalidAddress = 0x02,

        InvalidDataValue = 0x03,

        FunctionError4 = 0x04,

        FunctionError5 = 0x05,

        FunctionError6 = 0x06,
    }
}