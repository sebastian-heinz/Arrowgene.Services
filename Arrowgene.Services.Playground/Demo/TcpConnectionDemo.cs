namespace Arrowgene.Services.Playground.Demo
{
    using Buffers;
    using System;
    using System.Net;
    using Logging;
    using Network.Tcp.Client;
    using Network.Tcp.Server; 
    using Network.TCP.Server.AsyncEvent;

    public class TcpConnectionDemo
    {
        public TcpConnectionDemo()
        {

            ITcpServer svr = new AsyncEventServer(IPAddress.Any, 2345, new Logger("a"));
            svr.Logger.LogWrite += Logger_LogWrite_Server;
            svr.ClientConnected += Svr_ClientConnected;
            svr.ClientDisconnected += Svr_ClientDisconnected;
            svr.ReceivedPacket += Svr_ServerReceivedPacket;
            svr.Start();

            ITcpClient cli = new TcpClient();
            cli.Logger.LogWrite += Logger_LogWrite_Client;
            cli.Connected += Cli_Connected;
            cli.Disconnected += Cli_Disconnected;
            cli.ReceivedPacket += Cli_ClientReceivedPacket;
            cli.Connect(IPAddress.Parse("127.0.0.1"), 2345, TimeSpan.Zero);

            Console.WriteLine("Press any key to send.");
            Console.ReadKey();

            cli.Send(new byte[9]);

            Console.WriteLine("Press any key to disconnect.");
            Console.ReadKey();

            cli.Disconnect();
            svr.Stop();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private void Cli_ClientReceivedPacket(object sender, Network.Tcp.Client.ReceivedPacketEventArgs e)
        {
            IBuffer data = e.Data;
            Console.WriteLine(string.Format("Client: received packet Size:{0}", data.Size));
        }

        private void Svr_ServerReceivedPacket(object sender, Network.Tcp.Server.ReceivedPacketEventArgs e)
        {
            IBuffer data = e.Data;
            Console.WriteLine(string.Format("Server: received packet Size:{0}", data.Size));
            e.Socket.Send(new byte[10]);
        }

        private void Logger_LogWrite_Client(object sender, Logging.LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Client Log: {0}", e.Log.Text));
        }

        private void Logger_LogWrite_Server(object sender, Logging.LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Server Log: {0}", e.Log.Text));
        }

        private void Cli_Disconnected(object sender, Network.Tcp.Client.DisconnectedEventArgs disconnectedEventArgs)
        {
            Console.WriteLine("Client Disconnected");
        }

        private void Cli_Connected(object sender, Network.Tcp.Client.ConnectedEventArgs connectedEventArgs)
        {
            Console.WriteLine("Client Connected");
        }

        private void Svr_ClientDisconnected(object sender, Network.Tcp.Server.DisconnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Server: Client Disconnected ({0})", e.Socket));
        }

        private void Svr_ClientConnected(object sender, Network.Tcp.Server.ConnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Server: Client Connected ({0})", e.Socket));
        }
    }
}