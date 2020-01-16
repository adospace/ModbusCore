using System;

namespace ModbusCore
{
    public class ModbusMemoryMap
    {
        public ModbusMemoryMap()
        {
            InputCoils = OutputCoils = new ModbusCoilsTable();
            InputRegisters = OutputRegisters = new ModbusRegistersTable();
        }

        public ModbusMemoryMap(IModbusCoilsTable inputCoils, IModbusCoilsTable outputCoils, IModbusRegistersTable inputRegisters, IModbusRegistersTable outputRegisters)
        {
            InputCoils = inputCoils ?? throw new ArgumentNullException(nameof(inputCoils));
            OutputCoils = outputCoils ?? throw new ArgumentNullException(nameof(outputCoils));
            InputRegisters = inputRegisters ?? throw new ArgumentNullException(nameof(inputRegisters));
            OutputRegisters = outputRegisters ?? throw new ArgumentNullException(nameof(outputRegisters));
        }

        public ModbusMemoryMap(IModbusCoilsTable coils, IModbusRegistersTable registers)
        {
            InputCoils = OutputCoils = coils ?? throw new ArgumentNullException(nameof(coils));
            InputRegisters = OutputRegisters = registers ?? throw new ArgumentNullException(nameof(registers));
        }

        public IModbusCoilsTable InputCoils { get; }
        public IModbusRegistersTable InputRegisters { get; }
        public IModbusCoilsTable OutputCoils { get; }
        public IModbusRegistersTable OutputRegisters { get; }

    }
}