namespace ModbusCore
{
    public enum ModbusFunctionCode : byte
    {
        ReadDiscreteInputs = 0x02,
        ReadCoils = 0x01,
        WriteSingleCoil = 0x05,
        WriteMultipleCoils = 0x0F,

        ReadInputRegisters = 0x04,
        ReadHoldingRegisters = 0x03,
        WriteSingleRegister = 0x06,
        WriteMultipleRegisters = 0x10,
        ReadWriteMultipleRegisters = 0x17,
        MaskWriteRegister = 0x16,
        ReadFiFoQueue = 0x18,

        //File record access
        ReadFileRecord = 0x14,

        WriteFileRecord = 0x15,

        //Diagnostics
        ReadExceptionStatus = 0x07,

        Diagnostic = 0x08,//SubCode= 00-18,20 decimal
        GetComEventCounter = 0x0B,
        GetComEventLog = 0x0C,
        ReportServerID = 0x11,
        ReadDeviceIdentification = 0x2B, //SubCode=14 decimal
    }
}