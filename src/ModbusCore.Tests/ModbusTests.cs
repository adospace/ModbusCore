using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ModbusCore.Tests
{
    [TestClass]
    public class ModbusTests
    {
        private static Stream _serverStream;
        private static Stream _clientStream;

        [AssemblyInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            var tcpServer = new TcpListener(IPAddress.Loopback, 12345);
            tcpServer.Start();


            var tcpClient = new TcpClient();

            TcpClient connectedClient = null;

            Task.WaitAll(new Task[] {
                tcpClient.ConnectAsync(IPAddress.Loopback, 12345),
                Task.Run(async () =>
                {
                    connectedClient = await tcpServer.AcceptTcpClientAsync();
                }) }, testContext.CancellationTokenSource.Token);

            if (testContext.CancellationTokenSource.IsCancellationRequested)
                return;

            _serverStream = connectedClient.GetStream();
            _clientStream = tcpClient.GetStream();
        }

        [AssemblyCleanup]
        public static void FinalizeTests()
        {
            _serverStream?.Close();
            _clientStream?.Close();
        }

        [TestMethod]
        public void MasterShouldReadCoilsFromSlave()
        {
            var clientMemory = new ModbusMemoryMap();
            var client = new ModbusClient(new ModbusRTUTransport(_clientStream));
            var clientDevice = new ModbusDevice(clientMemory, 4);

            var serverMemory = new ModbusMemoryMap();
            var server = new ModbusServer(new ModbusRTUTransport(_serverStream));
            var serverDevice = new ModbusDevice(serverMemory, 4);

            serverMemory.OutputCoils[10] = true;
            serverMemory.OutputCoils[15] = true;

            clientMemory.OutputCoils[10].ShouldBeFalse();
            clientMemory.OutputCoils[15].ShouldBeFalse();

            Task.Run(() => server.HandleRequest(serverDevice));

            client.ReadCoils(clientDevice, 10, 13);

            //slave memory not touched
            serverMemory.OutputCoils[10].ShouldBeTrue();
            serverMemory.OutputCoils[15].ShouldBeTrue();

            //master memory is synched
            clientMemory.OutputCoils[10].ShouldBeTrue();
            clientMemory.OutputCoils[15].ShouldBeTrue();
        }

        [TestMethod]
        public void MasterShouldReadDiscreteInputsFromSlave()
        {
            var clientMemory = new ModbusMemoryMap();
            var client = new ModbusClient(new ModbusRTUTransport(_clientStream));
            var clientDevice = new ModbusDevice(clientMemory, 4);

            var serverMemory = new ModbusMemoryMap();
            var server = new ModbusServer(new ModbusRTUTransport(_serverStream));
            var serverDevice = new ModbusDevice(serverMemory, 4);

            serverMemory.InputCoils[10] = true;
            serverMemory.InputCoils[15] = true;

            clientMemory.InputCoils[10].ShouldBeFalse();
            clientMemory.InputCoils[15].ShouldBeFalse();

            Task.Run(() => server.HandleRequest(serverDevice));

            //read only one input
            client.ReadDiscreteInputs(clientDevice, 10, 1);

            //slave memory not touched
            serverMemory.InputCoils[10].ShouldBeTrue();
            serverMemory.InputCoils[15].ShouldBeTrue();

            //master memory is synched
            clientMemory.InputCoils[10].ShouldBeTrue();
            clientMemory.InputCoils[15].ShouldBeFalse();

            Task.Run(() => server.HandleRequest(serverDevice));

            //read another input
            client.ReadDiscreteInputs(clientDevice, 15, 1);

            //master memory is synched
            clientMemory.InputCoils[10].ShouldBeTrue();
            clientMemory.InputCoils[15].ShouldBeTrue();
        }

        //[TestMethod]
        //public void MasterShouldReadHoldingRegistersFromSlave()
        //{
        //    var dummyStreamConnectedToSlave = new MockedMemoryStream();

        //    var masterMemory = new ModbusMemoryMap();
        //    var master = new ModbusRTUTransport(masterMemory, dummyStreamConnectedToSlave, 4);

        //    var slaveMemory = new ModbusMemoryMap();
        //    var slave = new ModbusRTUTransport(slaveMemory, dummyStreamConnectedToSlave, 4);

        //    slaveMemory.OutputRegisters[10] = 23;
        //    slaveMemory.OutputRegisters[15] = 45;

        //    master.SendReadHoldingRegistersRequest(10, 13); //10...22 (Coil 11 to 23)



        //    slave.HandleReadHoldingRegistersRequest(out var offset, out var count);

        //    offset.ShouldBe(10);
        //    count.ShouldBe(13);



        //    slave.HandleReadHoldingRegistersResponse(offset, count);



        //    master.ReceiveReadHoldingRegistersResponse(offset, count);

        //    //slave memory not touched
        //    slaveMemory.OutputRegisters[10].ShouldBe(23);
        //    slaveMemory.OutputRegisters[15].ShouldBe(45);

        //    //master memory is synched
        //    masterMemory.OutputRegisters[10].ShouldBe(23);
        //    masterMemory.OutputRegisters[15].ShouldBe(45);
        //}

        //[TestMethod]
        //public void MasterShouldReadInputRegistersFromSlave()
        //{
        //    var dummyStreamConnectedToSlave = new MockedMemoryStream();

        //    var masterMemory = new ModbusMemoryMap();
        //    var master = new ModbusRTUTransport(masterMemory, dummyStreamConnectedToSlave, 4);

        //    var slaveMemory = new ModbusMemoryMap();
        //    var slave = new ModbusRTUTransport(slaveMemory, dummyStreamConnectedToSlave, 4);

        //    slaveMemory.InputRegisters[12] = 23;
        //    slaveMemory.InputRegisters[13] = 45;

        //    master.SendReadInputRegistersRequest(12, 3); //10...22 (Coil 11 to 23)



        //    slave.HandleReadInputRegistersRequest(out var offset, out var count);

        //    offset.ShouldBe(12);
        //    count.ShouldBe(3);



        //    slave.HandleReadInputRegistersResponse(offset, count);



        //    master.ReceiveReadInputRegistersResponse(offset, count);

        //    //slave memory not touched
        //    slaveMemory.InputRegisters[12].ShouldBe(23);
        //    slaveMemory.InputRegisters[13].ShouldBe(45);

        //    //master memory is synched
        //    masterMemory.InputRegisters[12].ShouldBe(23);
        //    masterMemory.InputRegisters[13].ShouldBe(45);
        //}

        //[TestMethod]
        //public void MasterShouldWriteSingleCoilToSlave()
        //{
        //    var dummyStreamConnectedToSlave = new MockedMemoryStream();

        //    var masterMemory = new ModbusMemoryMap();
        //    var master = new ModbusRTUTransport(masterMemory, dummyStreamConnectedToSlave, 4);

        //    var slaveMemory = new ModbusMemoryMap();
        //    var slave = new ModbusRTUTransport(slaveMemory, dummyStreamConnectedToSlave, 4);

        //    slaveMemory.OutputCoils[29].ShouldBeFalse();

        //    master.SendWriteSingleCoilRequest(29, true);



        //    var offset = slave.HandleWriteSingleCoilRequest();

        //    offset.ShouldBe(29);



        //    slave.HandleWriteSingleCoilResponse(offset);



        //    master.ReceiveWriteSingleCoilResponse(offset);

        //    //slave memory is updated
        //    slaveMemory.OutputCoils[29].ShouldBeTrue();



        //    master.SendReadCoilsRequest(offset, 1);



        //    slave.HandleReadCoilsRequest(out var offsetRequested, out var countRequested);

        //    offsetRequested.ShouldBe(offset);
        //    countRequested.ShouldBe(1);



        //    slave.HandleReadCoilsResponse(offset, 1);



        //    master.ReceiveReadCoilsResponse(offset, 1);

        //    //master memory is synched
        //    masterMemory.OutputCoils[29].ShouldBeTrue();
        //}

        //[TestMethod]
        //public void MasterShouldWriteSingleRegisterToSlave()
        //{
        //    var dummyStreamConnectedToSlave = new MockedMemoryStream();

        //    var masterMemory = new ModbusMemoryMap();
        //    var master = new ModbusRTUTransport(masterMemory, dummyStreamConnectedToSlave, 4);

        //    var slaveMemory = new ModbusMemoryMap();
        //    var slave = new ModbusRTUTransport(slaveMemory, dummyStreamConnectedToSlave, 4);

        //    slaveMemory.OutputRegisters[29].ShouldBe(0);

        //    master.SendWriteSingleRegisterRequest(29, 542);



        //    var offset = slave.HandleWriteSingleRegisterRequest();

        //    offset.ShouldBe(29);



        //    slave.HandleWriteSingleRegisterResponse(offset);



        //    master.ReceiveWriteSingleRegisterResponse(offset);

        //    //slave memory is updated
        //    slaveMemory.OutputRegisters[29].ShouldBe(542);



        //    master.SendReadHoldingRegistersRequest(offset, 1);



        //    slave.HandleReadHoldingRegistersRequest(out var offsetRequested, out var countRequested);

        //    offsetRequested.ShouldBe(offset);
        //    countRequested.ShouldBe(1);



        //    slave.HandleReadHoldingRegistersResponse(offset, 1);



        //    master.ReceiveReadHoldingRegistersResponse(offset, 1);

        //    //master memory is synched
        //    masterMemory.OutputRegisters[29].ShouldBe(542);
        //}

        //[TestMethod]
        //public void MasterShouldWriteMultipleCoilsToSlave()
        //{
        //    var dummyStreamConnectedToSlave = new MockedMemoryStream();

        //    var masterMemory = new ModbusMemoryMap();
        //    var master = new ModbusRTUTransport(masterMemory, dummyStreamConnectedToSlave, 4);

        //    var slaveMemory = new ModbusMemoryMap();
        //    var slave = new ModbusRTUTransport(slaveMemory, dummyStreamConnectedToSlave, 4);

        //    slaveMemory.OutputCoils[29].ShouldBeFalse();
        //    slaveMemory.OutputCoils[30].ShouldBeFalse();
        //    slaveMemory.OutputCoils[31].ShouldBeFalse();

        //    master.SendWriteMultipleCoilsRequest(29, true, false, true);



        //    slave.HandleWriteMultipleCoilsRequest(out var offset, out var countOfValues);

        //    offset.ShouldBe(29);
        //    countOfValues.ShouldBe(3);



        //    slave.HandleWriteMultipleCoilsResponse(offset, countOfValues);



        //    master.ReceiveWriteMultipleCoilsResponse(offset);//, out countOfValues);

        //    countOfValues.ShouldBe(3);

        //    //slave memory is updated
        //    slaveMemory.OutputCoils[29].ShouldBeTrue();
        //    slaveMemory.OutputCoils[30].ShouldBeFalse();
        //    slaveMemory.OutputCoils[31].ShouldBeTrue();

        //    //master memory is synched
        //    slaveMemory.OutputCoils[29].ShouldBeTrue();
        //    slaveMemory.OutputCoils[30].ShouldBeFalse();
        //    slaveMemory.OutputCoils[31].ShouldBeTrue();
        //}

        //[TestMethod]
        //public void MasterShouldWriteMultipleRegistersToSlave()
        //{
        //    var dummyStreamConnectedToSlave = new MockedMemoryStream();

        //    var masterMemory = new ModbusMemoryMap();
        //    var master = new ModbusRTUTransport(masterMemory, dummyStreamConnectedToSlave, 4);

        //    var slaveMemory = new ModbusMemoryMap();
        //    var slave = new ModbusRTUTransport(slaveMemory, dummyStreamConnectedToSlave, 4);

        //    slaveMemory.OutputRegisters[35].ShouldBe(0);
        //    slaveMemory.OutputRegisters[36].ShouldBe(0);
        //    slaveMemory.OutputRegisters[37].ShouldBe(0);
        //    slaveMemory.OutputRegisters[38].ShouldBe(0);

        //    master.SendWriteMultipleRegistersRequest(35, 123, 2, 7, 15);



        //    slave.HandleWriteMultipleRegistersRequest(out var offset, out var countOfValues);

        //    offset.ShouldBe(35);
        //    countOfValues.ShouldBe(4);



        //    slave.HandleWriteMultipleRegistersResponse(offset, countOfValues);



        //    master.ReceiveWriteMultipleRegistersResponse(offset);//, out countOfValues);

        //    countOfValues.ShouldBe(4);

        //    //slave memory is updated
        //    slaveMemory.OutputRegisters[35].ShouldBe(123);
        //    slaveMemory.OutputRegisters[36].ShouldBe(2);
        //    slaveMemory.OutputRegisters[37].ShouldBe(7);
        //    slaveMemory.OutputRegisters[38].ShouldBe(15);

        //    //master memory is synched
        //    slaveMemory.OutputRegisters[35].ShouldBe(123);
        //    slaveMemory.OutputRegisters[36].ShouldBe(2);
        //    slaveMemory.OutputRegisters[37].ShouldBe(7);
        //    slaveMemory.OutputRegisters[38].ShouldBe(15);
        //}

    }
}