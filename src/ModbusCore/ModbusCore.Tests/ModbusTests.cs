using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;

namespace ModbusCore.Tests
{
    [TestClass]
    public class ModbusTests
    {
        [TestMethod]
        public void MasterShouldReadCoilsFromSlave()
        {
            var dummyStreamConnectedToSlave = new MemoryStream();

            var masterMemory = new ModbusMemoryMap();
            var master = new ModbusRTUMaster(masterMemory, dummyStreamConnectedToSlave, 4);

            var slaveMemory = new ModbusMemoryMap();
            var slave = new ModbusRTUSlave(slaveMemory, dummyStreamConnectedToSlave, 4);

            slaveMemory.InputCoils[10] = true;
            slaveMemory.InputCoils[15] = true;

            master.SendReadCoilsRequest(10, 13); //10...22 (Coil 11 to 23)

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            slave.HandleReadCoilsRequest(out var offset, out var count);

            offset.ShouldBe(10);
            count.ShouldBe(13);

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            slave.HandleReadCoilsResponse(offset, count);

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            master.ReceiveReadCoilsResponse(offset, count);

            //slave memory not touched
            slaveMemory.InputCoils[10].ShouldBeTrue();
            slaveMemory.InputCoils[15].ShouldBeTrue();

            //master memory is synched
            masterMemory.InputCoils[10].ShouldBeTrue();
            masterMemory.InputCoils[15].ShouldBeTrue();
        }
    }
}