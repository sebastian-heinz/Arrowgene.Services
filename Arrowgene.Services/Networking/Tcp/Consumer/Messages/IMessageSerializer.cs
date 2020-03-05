namespace Arrowgene.Services.Networking.Tcp.Consumer.Messages
{
    public interface IMessageSerializer
    {
        byte[] Serialize(Message message);
    }
}