using System.Text;

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

        public string ToString(int index, int count)
        {
            Validate.Between(nameof(index), index, 0, count);
            Validate.Between(nameof(count), count, 0, Length - index);

            var sb = new StringBuilder();

            for (int i = 0; i < count; i++)
                sb.Append(_table[i].ToString("X") + " ");

            return sb.ToString();
        }
    }
}