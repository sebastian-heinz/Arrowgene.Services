using System;
using Arrowgene.Services.Networking.Tcp.Client;
using Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Models;
using Arrowgene.Services.Protocols.Messages;

namespace Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Client
{
    public class HandleLogin : MessageHandle<Login, ITcpClient>
    {
        public override int Id => Login.LoginId;

        protected override void Handle(Login message, ITcpClient token)
        {
            Console.WriteLine(string.Format("Client: HandleLogin for {0} {1}", message.Name, message.Hash));
        }
    }
}