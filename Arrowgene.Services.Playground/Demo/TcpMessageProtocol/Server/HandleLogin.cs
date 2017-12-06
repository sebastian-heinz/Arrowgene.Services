using System;
using Arrowgene.Services.Networking.Tcp;
using Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Models;
using Arrowgene.Services.Protocols.Messages;

namespace Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Server
{
    public class HandleLogin : MessageHandle<Login, ITcpSocket>
    {
        public override int Id => Login.LoginId;

        public HandleLogin()
        {
        }

        protected override void Handle(Login message, ITcpSocket token)
        {
            Console.WriteLine(string.Format("Server: HandleLogin for {0} {1}", message.Name, message.Hash));

            // echo data back
            byte[] data = Handler.Create(message);
            token.Send(data);
        }
    }
}