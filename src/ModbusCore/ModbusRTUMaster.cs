using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore
{
    //public class ModbusRTUMaster : ModbusMasterDevice, IModbusMaster
    //{
    //    public ModbusRTUMaster(ModbusMemoryMap memoryMap, Stream stream, byte address, IPacketLogger? packetLogger = null)
    //        : base(new ModbusMasterTransport(memoryMap, stream, address, packetLogger))
    //    {
    //    }

    //    protected override void OnEndReceivingMessage(IMessageBufferReader messageBufferReader)
    //    {
    //        CheckCrcIsValid(messageBufferReader);

    //        base.OnEndReceivingMessage(messageBufferReader);
    //    }

    //    protected override void OnEndSendingMessage(IMessageBufferWriter messageBufferWriter)
    //    {
    //        AppendCrc(messageBufferWriter);

    //        base.OnEndSendingMessage(messageBufferWriter);
    //    }

    //    protected void AppendCrc(IMessageBufferWriter messageBufferWriter)
    //    {
    //        CrcUtils.AppendCrc(messageBufferWriter, _messageBuffer);

    //        if (PacketLogger != null)
    //        {
    //            messageBufferWriter.Log(PacketLogger);
    //        }        
    //    }

    //    protected void CheckCrcIsValid(IMessageBufferReader messageBufferReader)
    //    {
    //        //check CRC
    //        messageBufferReader.PushFromStream(2);

    //        if (PacketLogger != null)
    //        {
    //            messageBufferReader.Log(PacketLogger);
    //        }

    //        CrcUtils.CheckCrcIsValid(_messageBuffer);
    //    }

    //}
}