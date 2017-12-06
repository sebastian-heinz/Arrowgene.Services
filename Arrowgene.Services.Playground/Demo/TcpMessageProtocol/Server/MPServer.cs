using System;
using System.Net;
using Arrowgene.Services.Networking.Tcp;
using Arrowgene.Services.Networking.Tcp.Server.AsyncEvent;
using Arrowgene.Services.Networking.Tcp.Server.EventConsumer.EventHandler;
using Arrowgene.Services.Protocols.Messages;

namespace Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Server
{
    public class MpServer
    {
        private AsyncEventServer _server;
        private MessageHandler<ITcpSocket> _handler;

        public MpServer()
        {
            EventHandlerConsumer serverConsumer = new EventHandlerConsumer();
            serverConsumer.ClientConnected += ServerConsumerOnClientConnected;
            serverConsumer.ClientDisconnected += ServerConsumerOnClientDisconnected;
            serverConsumer.ReceivedPacket += ServerConsumerOnReceivedPacket;
            _server = new AsyncEventServer(IPAddress.Any, 2345, serverConsumer);
            _handler = new MessageHandler<ITcpSocket>();
            _handler.AddHandle(new HandleLogin());
        }

        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server.Stop();
        }

        private void ServerConsumerOnReceivedPacket(object sender, ReceivedPacketEventArgs e)
        {
            _handler.Handle(e.Data, e.Socket);
        }

        private void ServerConsumerOnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine("Server: Client disconnected");
        }

        private void ServerConsumerOnClientConnected(object sender, ConnectedEventArgs e)
        {
            Console.WriteLine("Server: Client connected");
        }
    }
}