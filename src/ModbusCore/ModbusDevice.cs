using System;
using System.IO;

namespace ModbusCore
{
    public class ModbusDevice
    {
        public ModbusDevice(ModbusMemoryMap memoryMap, byte address)
        {
            MemoryMap = memoryMap ?? throw new ArgumentNullException(nameof(memoryMap));
            Address = address;
        }

        public ModbusMemoryMap MemoryMap { get; }
        public byte Address { get; }
    }
}