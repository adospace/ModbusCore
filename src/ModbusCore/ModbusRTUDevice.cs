using System.IO;

namespace ModbusCore
{
    public abstract class ModbusRTUDevice : ModbusDevice
    {
        protected ModbusRTUDevice(ModbusMemoryMap memoryMap, Stream stream, byte address, IPacketLogger? packetLogger = null)
            : base(memoryMap, stream, address, packetLogger)
        {
        }
    }
}