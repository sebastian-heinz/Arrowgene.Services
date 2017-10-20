namespace Arrowgene.Services.Playground.Demo
{
    using Common.Buffers;
    using Network.TCP.Client;
    using Network.TCP.Server;
    using System;
    using System.Net;

    public class TcpConnectionDemo
    {
        public TcpConnectionDemo()
        {
            TCPServer svr = new TCPServer(IPAddress.Any, 2345);
            svr.Logger.LogWrite += Logger_LogWrite_Server;
            svr.ClientConnected += Svr_ClientConnected;
            svr.ClientDisconnected += Svr_ClientDisconnected;
            svr.ReceivedPacket += Svr_ServerReceivedPacket;
            svr.Start();

            TCPClient cli = new TCPClient();
            cli.Logger.LogWrite += Logger_LogWrite_Client;
            cli.Connected += Cli_Connected;
            cli.Disconnected += Cli_Disconnected;
            cli.ReceivedPacket += Cli_ClientReceivedPacket;
            cli.Connect(IPAddress.Parse("192.168.178.20"), 2345);

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

        private void Cli_ClientReceivedPacket(object sender, ClientReceivedPacketEventArgs e)
        {
            IBuffer data = e.Payload;
            Console.WriteLine(string.Format("Client: received packet Size:{0}", data.Size));
        }

        private void Svr_ServerReceivedPacket(object sender, ReceivedPacketEventArgs e)
        {
            IBuffer data = e.Payload;
            Console.WriteLine(string.Format("Server: received packet Size:{0}", data.Size));
            e.ClientSocket.Send(new byte[10]);
        }

        private void Logger_LogWrite_Client(object sender, Logging.LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Client Log: {0}", e.Log.Text));
        }

        private void Logger_LogWrite_Server(object sender, Logging.LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Server Log: {0}", e.Log.Text));
        }

        private void Cli_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine("Client Disconnected");
        }

        private void Cli_Connected(object sender, ConnectedEventArgs e)
        {
            Console.WriteLine("Client Connected");
        }

        private void Svr_ClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Server: Client Disconnected ({0})", e.ClientSocket.Id));
        }

        private void Svr_ClientConnected(object sender, ConnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Server: Client Connected ({0})", e.ClientSocket.Id));
        }
    }
}