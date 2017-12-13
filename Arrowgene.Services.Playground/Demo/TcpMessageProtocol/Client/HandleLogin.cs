using System;
using Arrowgene.Services.Networking.Tcp;
using Arrowgene.Services.Networking.Tcp.Consumer.Messages;
using Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Models;

namespace Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Client
{
    public class HandleLogin : MessageHandle<Login>
    {
        public override int Id => Login.LoginId;

        protected override void Handle(Login message, ITcpSocket socket)
        {
            Console.WriteLine(string.Format("Client: HandleLogin for {0} {1}", message.Name, message.Hash));
        }
    }
}