using System.Net;
using Arrowgene.Networking.Tcp.Server.AsyncEvent;
using Arrowgene.Services.Networking.Tcp.Consumer.Messages;

namespace Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Server
{
    public class MpServer
    {
        private AsyncEventServer _server;

        public MpServer()
        {
            MessageConsumer serverConsumer = new MessageConsumer();
            serverConsumer.AddHandle(new HandleLogin());
            _server = new AsyncEventServer(IPAddress.Any, 2345, serverConsumer);
        }

        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server.Stop();
        }
    }
}