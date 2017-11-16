namespace Arrowgene.Services.Playground.Demo
{
    using Buffers;
    using System;
    using System.Net;
    using Logging;
    using Network.Tcp.Client;
    using Network.Tcp.Server;
    using Network.TCP.Server.AsyncEvent;

    public class TcpEchoDemo
    {
        public TcpEchoDemo()
        {
            ITcpServer svr = new AsyncEventServer(IPAddress.Any, 2345, new Logger("a"));
            svr.Logger.LogWrite += Logger_LogWrite_Server;
            svr.ClientConnected += Svr_ClientConnected;
            svr.ClientDisconnected += Svr_ClientDisconnected;
            svr.ReceivedPacket += Svr_ServerReceivedPacket;
            svr.Start();

            Console.WriteLine("Demo: Press any key to exit.");
            Console.ReadKey();
            svr.Stop();
        }

        private void Svr_ServerReceivedPacket(object sender, Network.Tcp.Server.ReceivedPacketEventArgs e)
        {
            IBuffer data = e.Data.Clone();
            Console.WriteLine(string.Format("Demo: Server: received packet Size:{0}", data.Size));
            e.Socket.Send(data.GetAllBytes());
        }

        private void Logger_LogWrite_Server(object sender, Logging.LogWriteEventArgs e)
        {
            Console.WriteLine(string.Format("Server Log: {0}", e.Log.Text));
        }

        private void Svr_ClientDisconnected(object sender, Network.Tcp.Server.DisconnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Demo: Server: Client Disconnected ({0})", e.Socket));
        }

        private void Svr_ClientConnected(object sender, Network.Tcp.Server.ConnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Demo: Server: Client Connected ({0})", e.Socket));
        }
    }
}