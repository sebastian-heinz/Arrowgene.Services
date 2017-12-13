using System;
using Arrowgene.Services.Networking.Tcp.Consumer.Messages;

namespace Arrowgene.Services.Playground.Demo.TcpMessageProtocol.Models
{
    [Serializable]
    public class Login : Message
    {
        public const int LoginId = 1;
        public override int Id => LoginId;
        public string Name { get; set; }
        public string Hash { get; set; }
    }
}