using System.Collections;

namespace ModbusCore
{
    public class ModbusCoilsTable : IModbusCoilsTable
    {
        private readonly BitArray _table = new BitArray(ushort.MaxValue);

        public ModbusCoilsTable()
        { }

        public bool this[int index]
        {
            get => _table[index];
            set => _table[index] = value;
        }
    }
}