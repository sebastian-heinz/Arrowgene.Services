using System;
using Arrowgene.Services.Networking.Tcp.Client;
using Arrowgene.Services.Networking.Tcp.Client.EventConsumer.EventHandler;
using Arrowgene.Services.Networking.Tcp.Client.SyncReceive;
using Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Models;
using Arrowgene.Services.Protocols.Messages;

namespace Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Client
{
    public class MpClient
    {
        private SyncReceiveTcpClient _client;
        private MessageHandler<ITcpClient> _handler;

        public MpClient()
        {
            EventHandlerConsumer clientConsumer = new EventHandlerConsumer();
            clientConsumer.ClientConnected += ClientConsumerOnClientConnected;
            clientConsumer.ClientDisconnected += ClientConsumerOnClientDisconnected;
            clientConsumer.ReceivedPacket += ClientConsumerOnReceivedPacket;
            clientConsumer.ConnectError += ClientConsumerOnConnectError;
            _client = new SyncReceiveTcpClient(clientConsumer);
            _handler = new MessageHandler<ITcpClient>();
            _handler.AddHandle(new HandleLogin());
        }

        public void Connect()
        {
            _client.Connect("127.0.0.1", 2345, TimeSpan.Zero);
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void Send(Message message)
        {
            byte[] data = _handler.Create(message);
            _client.Send(data);
        }

        private void ClientConsumerOnConnectError(object sender, ConnectErrorEventArgs e)
        {
            Console.WriteLine("Client: Connect Error");
        }

        private void ClientConsumerOnReceivedPacket(object sender, ReceivedPacketEventArgs e)
        {
            _handler.Handle(e.Data, e.Client);
        }

        private void ClientConsumerOnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine("Client: Disconnected");
        }

        private void ClientConsumerOnClientConnected(object sender, ConnectedEventArgs e)
        {
            Console.WriteLine("Client: Connected");
        }
    }
}