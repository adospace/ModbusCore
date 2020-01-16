namespace ModbusCore
{
    public interface IModbusRegistersTable
    {
        int this[int index] { get; set; }

        int Length { get; }

    }
}