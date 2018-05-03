using System;

namespace Arrowgene.Services.Networking.ServerBridge.Messages
{
    [Serializable]
    public class Message
    {
        public Guid Id { get; }


        public Message()
        {
            Id = Guid.NewGuid();
        }

        public Message(Guid id)
        {
            Id = id;
        }
    }
}