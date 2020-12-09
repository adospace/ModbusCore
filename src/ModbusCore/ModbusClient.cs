using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    public partial class ModbusClient
    {
        public ModbusClient(
            ModbusTransport modbusTransport,
            IPacketLogger? packetLogger = null)
        {
            ModbusTransport = modbusTransport;
            PacketLogger = packetLogger;
        }

        internal ModbusTransport ModbusTransport { get; }
        public IPacketLogger? PacketLogger { get; }
        protected virtual int GetTransactionIdentifier() => 0;
    }

}
