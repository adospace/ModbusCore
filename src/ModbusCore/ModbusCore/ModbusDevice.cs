using System;
using System.IO;

namespace ModbusCore
{
    public abstract class ModbusDevice
    {
        protected ModbusDevice(ModbusMemoryMap memoryMap, Stream stream, byte address)
        {
            MemoryMap = memoryMap ?? throw new ArgumentNullException(nameof(memoryMap));
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            Address = address;
        }

        public ModbusMemoryMap MemoryMap { get; }
        public Stream Stream { get; }
        public byte Address { get; }
    }
}