using CommandLine;
using System;
using System.Net.Sockets;
using System.Threading;

namespace ModbusCore.Demo
{
	public enum CommunicationMode
	{ 
		RTU_SERIAL,

		ASCI_SERIAL,

		RTU_TCP,

		TCP
	}

    public class Program
    {
		[Verb("slave", HelpText = "Run as slave device polled from a master device.")]
		class SlaveOptions
		{
			//normal options here
		}

		[Verb("master", HelpText = "Run as master device polling other slave devices.")]
		class MasterOptions
		{
			[Option('m', "mode", HelpText = "Communication mode: RTU_SERIAL, ASCI_SERIAL, RTU_TCP, TCP")]
			public CommunicationMode Mode { get; set; }

			[Option("host", HelpText = "Slave tcp host")]
			public string Host { get; set; }

			[Option("port", HelpText = "Slave tcp port")]
			public int Port { get; set; } = 502;

			[Option("id", HelpText = "Slave device id")]
			public byte DeviceId { get; set; }

			[Option("coils-offset", HelpText = "Coils 0-based start offset to read")]
			public int CoilsOffset { get; set; }

			[Option("coils-count", HelpText = "Coils count to read")]
			public int CoilsCount { get; set; }
		}

		public static void Main(string[] args)
		{
			var cancellationTokenSource = new CancellationTokenSource();

			Parser.Default.ParseArguments<SlaveOptions, MasterOptions>(args)
			  .WithParsed<MasterOptions>((opts) => RunAsMaster(opts, cancellationTokenSource.Token))
			  .WithNotParsed((errs) => throw new InvalidOperationException());

			Console.CancelKeyPress += (s, e) => 
			{
				cancellationTokenSource.Cancel();
			};

			cancellationTokenSource.Token.WaitHandle.WaitOne();
		}

        private static void RunAsMaster(MasterOptions options, CancellationToken cancellationToken)
        {
            switch (options.Mode)
            {
                case CommunicationMode.RTU_SERIAL:
					throw new NotSupportedException();
				case CommunicationMode.ASCI_SERIAL:
					throw new NotSupportedException();
				case CommunicationMode.RTU_TCP:
					throw new NotSupportedException();
				case CommunicationMode.TCP:
                    RunAsTcpMaster(options, cancellationToken);
					break;
                default:
					throw new NotSupportedException();
			}
        }

        private static async void RunAsTcpMaster(MasterOptions options, CancellationToken cancellationToken)
		{
			var memoryMap = new ModbusMemoryMap();

			using var tcpClient = new TcpClient();
			await tcpClient.ConnectAsync(options.Host, options.Port);

			var master = new ModbusTCPMaster(memoryMap, tcpClient.GetStream(), options.DeviceId);

			while (!cancellationToken.IsCancellationRequested)
			{
				if (options.CoilsCount > 0)
				{ 
					
				}
			
			}			
		}

        private static int RunAsSlave(SlaveOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
