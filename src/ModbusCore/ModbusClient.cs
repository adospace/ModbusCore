using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public abstract class ModbusClient
    {
        protected ModbusClient(
            ModbusTransport modbusTransport,
            IPacketLogger? packetLogger = null)
        {
            ModbusTransport = modbusTransport;
            PacketLogger = packetLogger;
        }

        public ModbusTransport ModbusTransport { get; }
        public IPacketLogger? PacketLogger { get; }

        public async Task PollAsync(ModbusDevice[] devices, CancellationToken cancellationToken)
        { 
            
        
        }

        public void Poll(params ModbusDevice[] devices)
        {

        }
    }
}
