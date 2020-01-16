namespace ModbusCore
{
    public class ModbusRegistersTable : IModbusRegistersTable
    {
        private readonly ushort[] _table = new ushort[ushort.MaxValue];

        public ModbusRegistersTable()
        { }

        public ushort this[int index]
        {
            get => _table[index];
            set => _table[index] = value;
        }
    }
}