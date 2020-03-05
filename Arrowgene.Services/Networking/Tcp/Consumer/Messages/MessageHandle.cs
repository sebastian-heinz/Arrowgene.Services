using Arrowgene.Networking.Tcp;

namespace Arrowgene.Services.Networking.Tcp.Consumer.Messages
{
    public abstract class MessageHandle<T> : IMessageHandle where T : Message
    {
        private IMessageSerializer _serializer;
        public abstract int Id { get; }

        protected abstract void Handle(T message, ITcpSocket socket);

        protected byte[] Serialize(Message message)
        {
            return _serializer.Serialize(message);
        }

        public void Process(Message message, ITcpSocket socket)
        {
            Handle((T) message, socket);
        }

        public void SetMessageSerializer(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }
    }
}