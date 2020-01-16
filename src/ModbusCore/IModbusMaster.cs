namespace ModbusCore
{
    public interface IModbusMaster
    {
        void ReadCoils(int zeroBasedOffset, int count);

        void ReadDiscreteInputs(int zeroBasedOffset, int count);

        void ReadHoldingRegisters(int zeroBasedOffset, int count);

        void ReadInputRegisters(int zeroBasedOffset, int count);
    }
}