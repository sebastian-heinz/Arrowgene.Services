namespace Arrowgene.Services.Playground.Demo
{
    using Buffers;
    using System;
    using System.Net;
    using Logging;
    using Network.Tcp.Client;
    using Network.Tcp.Server;
    using Network.Tcp.Server.AsyncEvent;
    using Network.Tcp.Server.EventConsumer.EventHandler;

    public class TcpConnectionDemo
    {
        public TcpConnectionDemo()
        {
            EventHandlerConsumer consumer = new EventHandlerConsumer();
            consumer.ClientConnected += Svr_ClientConnected;
            consumer.ClientDisconnected += Svr_ClientDisconnected;
            consumer.ReceivedPacket += Svr_ServerReceivedPacket;

            ITcpServer svr = new AsyncEventServer(IPAddress.Any, 2345, consumer, new Logger("a"));
            svr.Logger.LogWrite += Logger_LogWrite_Server;
            svr.Start();

            ITcpClient cli = new TcpClient();
            LogProvider.LogWrite += Logger_LogWrite_Client;
            cli.Connected += Cli_Connected;
            cli.Disconnected += Cli_Disconnected;
            cli.ReceivedPacket += Cli_ClientReceivedPacket;
            cli.Connect(IPAddress.Parse("127.0.0.1"), 2345, TimeSpan.Zero);

            Console.WriteLine("Demo: Press any key to send.");
            Console.ReadKey();

            cli.Send(new byte[9]);

            Console.WriteLine("Demo: Press any key to disconnect.");
            Console.ReadKey();

            cli.Disconnect();
            svr.Stop();

            Console.WriteLine("Demo: Press any key to exit.");
            Console.ReadKey();
        }


        private void Cli_ClientReceivedPacket(object sender, Network.Tcp.Client.ReceivedPacketEventArgs e)
        {
            IBuffer data = e.Data;
            Console.WriteLine(string.Format("Demo: Client: received packet Size:{0}", data.Size));
        }

        private void Svr_ServerReceivedPacket(object sender, Network.Tcp.Server.EventConsumer.EventHandler.ReceivedPacketEventArgs e)
        {
            byte[] received = e.Data;
            Console.WriteLine(string.Format("Demo: Server: received packet Size:{0}", received.Length));
            e.Socket.Send(new byte[10]);
        }

        private void Logger_LogWrite_Client(object sender, LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Client Log: {0}", e.Log.Text));
        }

        private void Logger_LogWrite_Server(object sender, LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Server Log: {0}", e.Log.Text));
        }

        private void Cli_Disconnected(object sender, Network.Tcp.Client.DisconnectedEventArgs disconnectedEventArgs)
        {
            Console.WriteLine("Demo: Client Disconnected");
        }

        private void Cli_Connected(object sender, Network.Tcp.Client.ConnectedEventArgs connectedEventArgs)
        {
            Console.WriteLine("Demo: Client Connected");
        }

        private void Svr_ClientDisconnected(object sender, Network.Tcp.Server.EventConsumer.EventHandler.DisconnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Demo: Server: Client Disconnected ({0})", e.Socket));
        }

        private void Svr_ClientConnected(object sender, Network.Tcp.Server.EventConsumer.EventHandler.ConnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Demo: Server: Client Connected ({0})", e.Socket));
        }
    }
}