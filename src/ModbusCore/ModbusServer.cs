using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public class ModbusServer
    {
        protected ModbusServer(
            ModbusTransport modbusTransport,
            IPacketLogger? packetLogger = null)
        {
            ModbusTransport = modbusTransport;
            PacketLogger = packetLogger;
        }

        public ModbusTransport ModbusTransport { get; }
        public IPacketLogger? PacketLogger { get; }

        public async Task ReceivePollingAsync(ModbusDevice[] devices, CancellationToken cancellationToken)
        {


        }

        public void ReceivePolling(params ModbusDevice[] devices)
        { 
        
        }
    }
}
