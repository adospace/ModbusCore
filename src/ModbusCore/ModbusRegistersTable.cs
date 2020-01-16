namespace ModbusCore
{
    public class ModbusRegistersTable : IModbusRegistersTable
    {
        private readonly int[] _table = new int[ushort.MaxValue];

        public ModbusRegistersTable()
        { }

        public int this[int index]
        {
            get => _table[index];
            set => _table[index] = value;
        }

        public int Length => _table.Length;
    }
}