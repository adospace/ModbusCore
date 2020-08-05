using System;
using System.Collections;
using System.Linq;
using System.Text;

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

        public int Length => _table.Length;

        public string ToString(int index, int count)
        {
            Validate.Between(nameof(index), index, 0, count);
            Validate.Between(nameof(count), count, 0, Length - index);

            var sb = new char[count];

            for (int i = 0; i < count; i++)
                sb[i] = _table[i] ? '1' : '0';

            return new string(sb);
        }
    }
}