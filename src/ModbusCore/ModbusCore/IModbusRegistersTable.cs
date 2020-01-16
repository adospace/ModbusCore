namespace ModbusCore
{
    public interface IModbusRegistersTable
    {
        ushort this[int index] { get; set; }
    }
}