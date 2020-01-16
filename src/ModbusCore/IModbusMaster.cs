namespace ModbusCore
{
    public interface IModbusMaster
    {
        void ReadCoils(int zeroBasedOffset, int count);

        void ReadDiscreteInputs(int zeroBasedOffset, int count);
    }
}