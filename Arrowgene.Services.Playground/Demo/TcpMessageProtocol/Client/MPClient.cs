using System;
using Arrowgene.Services.Networking.Tcp.Client;
using Arrowgene.Services.Networking.Tcp.Client.SyncReceive;
using Arrowgene.Services.Networking.Tcp.Consumer.Messages;

namespace Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Client
{
    public class MpClient
    {
        private SyncReceiveTcpClient _client;
        private IMessageSerializer _serializer;

        public MpClient()
        {
            MessageConsumer clientConsumer = new MessageConsumer();
            clientConsumer.AddHandle(new HandleLogin());
            _serializer = clientConsumer;
            
            _client = new SyncReceiveTcpClient(clientConsumer);
            _client.ConnectError += ClientConsumerOnConnectError;
        }

        public void Connect()
        {
            _client.Connect("127.0.0.1", 2345, TimeSpan.Zero);
        }

        public void Disconnect()
        {
            _client.Close();
        }

        public void Send(Message message)
        {
            byte[] data = _serializer.Serialize(message);
            _client.Send(data);
        }

        private void ClientConsumerOnConnectError(object sender, ConnectErrorEventArgs e)
        {
            Console.WriteLine("Client: Connect Error");
        }
    }
}