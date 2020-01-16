namespace ModbusCore
{
    public interface IModbusMaster
    {
        void ReadCoils(int zeroBasedOffset, int count);
    }
}