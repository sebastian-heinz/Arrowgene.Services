using System;

namespace Arrowgene.Services.Networking.Tcp.Consumer.Messages
{
    [Serializable]
    public abstract class Message
    {
        public abstract int Id { get; }
    }
}