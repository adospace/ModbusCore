using CommandLine;
using Konsole;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusCore.TestApp
{
    class Program
    {
        public class Options
        {
            [Option('h', "host", Required = false, HelpText = "TCP slave host.")]
            public string Host { get; set; } = "127.0.0.1";

            [Option('p', "port", Required = false, HelpText = "TCP slave port.")]
            public int Port { get; set; } = 502;

            [Option("input_count", Required = false, HelpText = "Input count to read.")]
            public int InputCount { get; set; } = 10;

            [Option("input_address", Required = false, HelpText = "Input address to start read.")]
            public int InputAddress { get; set; } = 0;

            [Option("coils_count", Required = false, HelpText = "Coils count to read.")]
            public int CoilsCount { get; set; } = 10;

            [Option("coils_address", Required = false, HelpText = "Coils address to start read.")]
            public int CoilsAddress { get; set; } = 0;

            [Option("register_count", Required = false, HelpText = "Input count to read.")]
            public int RegisterCount { get; set; } = 10;

            [Option("register_address", Required = false, HelpText = "Input address to start read.")]
            public int RegisterAddress { get; set; } = 0;

            [Option("holding_count", Required = false, HelpText = "Holding register count to read.")]
            public int HoldingRegisterCount { get; set; } = 10;

            [Option("holding_address", Required = false, HelpText = "Holding registers address to start read.")]
            public int HoldingRegisterAddress { get; set; } = 0;

            [Option('s', "slave", Required = false, HelpText = "Slave id.")]
            public byte SlaveId { get; set; } = 1;

            [Option("scan_rate", Required = false, HelpText = "Slave scan rate in millisecond.")]
            public int ScanRate { get; set; } = 500;

        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
               .WithParsed<Options>(async o =>
               {
                   await Run(o);
               });

            Console.ReadKey();
        }

        private static async Task Run(Options options)
        {
            var connectionWindow = Window.OpenBox("Connection", 80, 4);
            var diWindow = Window.OpenBox("Input", 80, 3);
            var doWindow = Window.OpenBox("Ouput", 80, 3);
            var inputRegistersWindow = Window.OpenBox("Resisters", 80, 3);
            var holdingRegistersWindow = Window.OpenBox("Holding Registers", 80, 3);


            while (!Console.KeyAvailable)
            {
                try
                {
                    using var socketToSlave = new TcpClient
                    {
                        ReceiveTimeout = 5000
                    };

                    await socketToSlave.ConnectAsync(IPAddress.Parse(options.Host), options.Port);

                    connectionWindow.Clear();
                    connectionWindow.PrintAt(0, 0, "Connected");

                    await ConnecionLoop(socketToSlave.GetStream(), options, diWindow, doWindow, inputRegistersWindow, holdingRegistersWindow);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                    connectionWindow.Clear();
                    connectionWindow.PrintAtColor(ConsoleColor.Red, 0, 0, ex.Message, ConsoleColor.Black);

                    await Task.Delay(options.ScanRate);
                }
            }
        }

        private static async Task ConnecionLoop(NetworkStream connectedStream, Options options, IConsole diWindow, IConsole doWindow, IConsole inputRegistersWindow, IConsole holdingRegistersWindow)
        {
            var masterMemory = new ModbusMemoryMap();
            var master = new ModbusRTUMaster(masterMemory, connectedStream, options.SlaveId);
            
            while (!Console.KeyAvailable)
            {
                if (options.InputCount > 0)
                {
                    await master.ReadDiscreteInputsAsync(options.InputAddress, options.InputCount, CancellationToken.None);
                    diWindow.PrintAt(0, 0, masterMemory.InputCoils.ToString(options.InputAddress, options.InputCount));
                }

                if (options.CoilsCount > 0)
                {
                    await master.ReadCoilsAsync(options.CoilsAddress, options.CoilsCount, CancellationToken.None);
                    doWindow.PrintAt(0, 0, masterMemory.OutputCoils.ToString(options.CoilsAddress, options.CoilsCount));
                }

                if (options.RegisterCount > 0)
                {
                    await master.ReadInputRegistersAsync(options.RegisterAddress, options.RegisterCount, CancellationToken.None);
                    inputRegistersWindow.PrintAt(0, 0, masterMemory.InputRegisters.ToString(options.RegisterAddress, options.RegisterCount));
                }

                if (options.HoldingRegisterCount > 0)
                {
                    await master.ReadHoldingRegistersAsync(options.HoldingRegisterAddress, options.HoldingRegisterCount, CancellationToken.None);
                    holdingRegistersWindow.PrintAt(0, 0, masterMemory.OutputRegisters.ToString(options.HoldingRegisterAddress, options.HoldingRegisterCount));
                }

                await Task.Delay(options.ScanRate);
            }
        }
    }
}
