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

            slaveMemory.OutputCoils[10] = true;
            slaveMemory.OutputCoils[15] = true;

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
            slaveMemory.OutputCoils[10].ShouldBeTrue();
            slaveMemory.OutputCoils[15].ShouldBeTrue();

            //master memory is synched
            masterMemory.OutputCoils[10].ShouldBeTrue();
            masterMemory.OutputCoils[15].ShouldBeTrue();
        }

        [TestMethod]
        public void MasterShouldReadDiscreteInputsFromSlave()
        {
            var dummyStreamConnectedToSlave = new MemoryStream();

            var masterMemory = new ModbusMemoryMap();
            var master = new ModbusRTUMaster(masterMemory, dummyStreamConnectedToSlave, 4);

            var slaveMemory = new ModbusMemoryMap();
            var slave = new ModbusRTUSlave(slaveMemory, dummyStreamConnectedToSlave, 4);

            slaveMemory.InputCoils[12] = true;
            slaveMemory.InputCoils[13] = true;

            master.SendReadDiscreteInputsRequest(10, 13); //10...22 (Coil 11 to 23)

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            slave.HandleReadDiscreteInputsRequest(out var offset, out var count);

            offset.ShouldBe(10);
            count.ShouldBe(13);

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            slave.HandleReadDiscreteInputsResponse(offset, count);

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            master.ReceiveReadDiscreteInputsResponse(offset, count);

            //slave memory not touched
            slaveMemory.InputCoils[12].ShouldBeTrue();
            slaveMemory.InputCoils[13].ShouldBeTrue();

            //master memory is synched
            masterMemory.InputCoils[12].ShouldBeTrue();
            masterMemory.InputCoils[13].ShouldBeTrue();
        }

        [TestMethod]
        public void MasterShouldReadHoldingRegistersFromSlave()
        {
            var dummyStreamConnectedToSlave = new MemoryStream();

            var masterMemory = new ModbusMemoryMap();
            var master = new ModbusRTUMaster(masterMemory, dummyStreamConnectedToSlave, 4);

            var slaveMemory = new ModbusMemoryMap();
            var slave = new ModbusRTUSlave(slaveMemory, dummyStreamConnectedToSlave, 4);

            slaveMemory.OutputRegisters[10] = 23;
            slaveMemory.OutputRegisters[15] = 45;

            master.SendReadHoldingRegistersRequest(10, 13); //10...22 (Coil 11 to 23)

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            slave.HandleReadHoldingRegistersRequest(out var offset, out var count);

            offset.ShouldBe(10);
            count.ShouldBe(13);

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            slave.HandleReadHoldingRegistersResponse(offset, count);

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            master.ReceiveReadHoldingRegistersResponse(offset, count);

            //slave memory not touched
            slaveMemory.OutputRegisters[10].ShouldBe((ushort)23);
            slaveMemory.OutputRegisters[15].ShouldBe((ushort)45);

            //master memory is synched
            masterMemory.OutputRegisters[10].ShouldBe((ushort)23);
            masterMemory.OutputRegisters[15].ShouldBe((ushort)45);
        }

        [TestMethod]
        public void MasterShouldReadInputRegistersFromSlave()
        {
            var dummyStreamConnectedToSlave = new MemoryStream();

            var masterMemory = new ModbusMemoryMap();
            var master = new ModbusRTUMaster(masterMemory, dummyStreamConnectedToSlave, 4);

            var slaveMemory = new ModbusMemoryMap();
            var slave = new ModbusRTUSlave(slaveMemory, dummyStreamConnectedToSlave, 4);

            slaveMemory.InputRegisters[12] = 23;
            slaveMemory.InputRegisters[13] = 45;

            master.SendReadInputRegistersRequest(12, 3); //10...22 (Coil 11 to 23)

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            slave.HandleReadInputRegistersRequest(out var offset, out var count);

            offset.ShouldBe(12);
            count.ShouldBe(3);

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            slave.HandleReadInputRegistersResponse(offset, count);

            dummyStreamConnectedToSlave.Seek(0, SeekOrigin.Begin);

            master.ReceiveReadInputRegistersResponse(offset, count);

            //slave memory not touched
            slaveMemory.InputRegisters[12].ShouldBe((ushort)23);
            slaveMemory.InputRegisters[13].ShouldBe((ushort)45);

            //master memory is synched
            masterMemory.InputRegisters[12].ShouldBe((ushort)23);
            masterMemory.InputRegisters[13].ShouldBe((ushort)45);
        }

    }
}