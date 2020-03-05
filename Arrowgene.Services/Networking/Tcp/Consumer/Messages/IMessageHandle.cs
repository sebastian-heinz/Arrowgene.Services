using Arrowgene.Networking.Tcp;

namespace Arrowgene.Services.Networking.Tcp.Consumer.Messages
{
    public interface IMessageHandle
    {
        int Id { get; }
        void Process(Message message, ITcpSocket socket);
        void SetMessageSerializer(IMessageSerializer serializer);
    }
}