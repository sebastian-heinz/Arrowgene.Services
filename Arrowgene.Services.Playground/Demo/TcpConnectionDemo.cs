using System;
using System.Net;
using Arrowgene.Services.Logging;
using Arrowgene.Services.Networking.Tcp.Client;
using Arrowgene.Services.Networking.Tcp.Client.SyncReceive;
using Arrowgene.Services.Networking.Tcp.Consumer.EventHandler;
using Arrowgene.Services.Networking.Tcp.Server.AsyncEvent;

namespace Arrowgene.Services.Playground.Demo
{
    public class TcpConnectionDemo
    {
        public TcpConnectionDemo()
        {
            LogProvider.GlobalLogWrite += LogProviderOnLogWrite;

            EventHandlerConsumer serverConsumer = new EventHandlerConsumer();
            serverConsumer.ClientConnected += ServerConsumerOnClientConnected;
            serverConsumer.ClientDisconnected += ServerConsumerOnClientDisconnected;
            serverConsumer.ReceivedPacket += ServerConsumerOnReceivedPacket;
            AsyncEventServer server = new AsyncEventServer(IPAddress.Any, 2345, serverConsumer);
            server.Start();

            EventHandlerConsumer clientConsumer = new EventHandlerConsumer();
            clientConsumer.ClientConnected += ClientConsumerOnClientConnected;
            clientConsumer.ClientDisconnected += ClientConsumerOnClientDisconnected;
            clientConsumer.ReceivedPacket += ClientConsumerOnReceivedPacket;
            SyncReceiveTcpClient client = new SyncReceiveTcpClient(clientConsumer);
            client.ConnectError += ClientConsumerOnConnectError;


            client.Connect(IPAddress.Parse("127.0.0.1"), 2345, TimeSpan.Zero);
            Console.WriteLine("Demo: Press any key to send.");
            Console.ReadKey();

            client.Send(new byte[9]);
            Console.WriteLine("Demo: Press any key to disconnect.");
            Console.ReadKey();

            client.Close();
            server.Stop();
            Console.WriteLine("Demo: Press any key to exit.");
            Console.ReadKey();
        }

        private void ServerConsumerOnReceivedPacket(object sender, ReceivedPacketEventArgs e)
        {
            byte[] received = e.Data;
            Console.WriteLine(string.Format("Demo: Server: received packet Size:{0}", received.Length));
            e.Socket.Send(new byte[10]);
        }

        private void ServerConsumerOnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Demo: Server: Client Disconnected ({0})", e.Socket));
        }

        private void ServerConsumerOnClientConnected(object sender, ConnectedEventArgs e)
        {
            Console.WriteLine(string.Format("Demo: Server: Client Connected ({0})", e.Socket));
        }

        private void ClientConsumerOnConnectError(object sender, ConnectErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ClientConsumerOnReceivedPacket(object sender, ReceivedPacketEventArgs e)
        {
            byte[] data = e.Data;
            Console.WriteLine(string.Format("Demo: Client: received packet Size:{0}", data.Length));
        }

        private void ClientConsumerOnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine("Demo: Client Connected");
        }

        private void ClientConsumerOnClientConnected(object sender, ConnectedEventArgs e)
        {
            Console.WriteLine("Demo: Client Disconnected");
        }

        private void LogProviderOnLogWrite(object sender, LogWriteEventArgs e)
        {
            Console.WriteLine(e.Log);
        }
    }
}