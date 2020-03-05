using System;
using Arrowgene.Networking.Tcp;
using Arrowgene.Services.Networking.Tcp;
using Arrowgene.Services.Networking.Tcp.Consumer.Messages;
using Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Models;

namespace Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Server
{
    public class HandleLogin : MessageHandle<Login>
    {
        public override int Id => Login.LoginId;

        public HandleLogin()
        {
        }

        protected override void Handle(Login message, ITcpSocket socket)
        {
            Console.WriteLine(string.Format("Server: HandleLogin for {0} {1}", message.Name, message.Hash));

            // echo data back
            byte[] data = Serialize(message);
            socket.Send(data);
        }
    }
}